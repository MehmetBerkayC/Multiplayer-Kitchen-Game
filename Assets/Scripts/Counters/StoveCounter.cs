using System;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State State;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOs;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOs;

    private NetworkVariable<State> _state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> _fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> _burningTimer = new NetworkVariable<float>(0f);
    FryingRecipeSO _fryingRecipeSO;
    BurningRecipeSO _burningRecipeSO;

    public override void OnNetworkSpawn()
    {
        _fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        _burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        _state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            State = _state.Value,
        });

        if (_state.Value == State.Burned || _state.Value == State.Idle) // To hide the progress bar
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                ProgressNormalized = 0f
            });
        }
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float fryingTimerMax = fryingRecipeSOs != null ? _fryingRecipeSO.FryingTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            ProgressNormalized = _fryingTimer.Value / fryingTimerMax
        });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = _burningRecipeSO != null ? _burningRecipeSO.BurningTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            ProgressNormalized = _burningTimer.Value / burningTimerMax
        });
    }

    private void Update()
    {
        if (!IsServer) return;

        if (HasKitchenObject())
        {
            switch (_state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    _fryingTimer.Value += Time.deltaTime; // You just use .Value for changing NetowrkVariables

                    // Automatically gets invoked on NetworkVariable change above OLD SINGLEPLAYER CODE EXAMPLE
                    //OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    //{
                    //    ProgressNormalized = _fryingTimer / _fryingRecipeSO.FryingTimerMax
                    //});

                    if (_fryingTimer.Value > _fryingRecipeSO.FryingTimerMax)
                    {
                        // Fried
                        KitchenObject.DestroyKitchenObject(GetKitchenObject()); // Destroy server side

                        KitchenObject.SpawnKitchenObject(_fryingRecipeSO.Output, this);

                        _state.Value = State.Fried;
                        _burningTimer.Value = 0;

                        _burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                        SetBurningRecipeSOClientRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO()));
                        // OLD SINGLEPLAYER CODE EXAMPLE
                        //OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        //{
                        //    State = _state,
                        //});
                    }
                    break;
                case State.Fried:
                    _burningTimer.Value += Time.deltaTime;

                    if (_burningTimer.Value > _burningRecipeSO.BurningTimerMax)
                    {
                        // Fried
                        KitchenObject.DestroyKitchenObject(GetKitchenObject()); // Destroy server side

                        KitchenObject.SpawnKitchenObject(_burningRecipeSO.Output, this);

                        _state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (HasKitchenObject())
        {
            // There is a KitchenObject on the counter
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // Player is holding a plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        SetStateIdleServerRpc();
                    }
                }
            }
            else
            {
                // Player has nothing
                GetKitchenObject().SetKitchenObjectParent(player); // Give to player
                SetStateIdleServerRpc();
            }
        }
        else
        {
            // there is no KitchenObject on the counter
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {   // Player is carrying something that can be cut
                    KitchenObject kitchenObject = player.GetKitchenObject(); // Caching to sync parent switch
                    kitchenObject.SetKitchenObjectParent(this); // Take from player

                    InteractLogicPlaceObjectOnCounterServerRpc(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(
                            kitchenObject.GetKitchenObjectSO()));
                }
            }
            else
            {
                // Player has nothing
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        _state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        _fryingTimer.Value = 0; // client can't write to a network variable

        _state.Value = State.Frying;

        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        // Placing object on the stove 
        _fryingRecipeSO = GetFryingRecipeSOWithInput(KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex));
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        // Placing object on the stove 
        _burningRecipeSO = GetBurningRecipeSOWithInput(KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex));
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO cuttingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.Output;
        }
        else
        {
            return null;
        }
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOs)
        {
            if (fryingRecipeSO.Input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null; // No matching recipe inputs
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOs)
        {
            if (burningRecipeSO.Input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null; // No matching recipe inputs
    }

    public bool IsFried()
    {
        return _state.Value == State.Fried;
    }
}

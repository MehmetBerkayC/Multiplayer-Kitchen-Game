public class ClearCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        if (HasKitchenObject())
        {
            // There is a KitchenObject on the counter
            if (player.HasKitchenObject())
            {
                // Player is carrying something
            }
            else
            {
                // Player has nothing
                GetKitchenObject().SetKitchenObjectParent(player); // Give to player
            }
        }
        else
        {
            // there is no KitchenObject on the counter
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this); // Take from player
            }
            else
            {
                // Player has nothing
            }
        }
    }
}

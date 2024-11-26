public class DeliveryCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
        {
            // Only accepts plates
            player.GetKitchenObject().DestroySelf();
        }
    }
}

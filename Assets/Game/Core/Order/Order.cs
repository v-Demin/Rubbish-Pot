namespace RubbishPot.Core
{
    public class Order
    {
        private IOrderRequest _request;

        public bool CheckForRequest(Potion potion)
        {
            return _request.Check(potion);
        }
    }
}

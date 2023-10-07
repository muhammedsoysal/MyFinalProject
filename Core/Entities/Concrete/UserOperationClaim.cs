namespace Core.Entities.Concrete
{
    public class UserOperationClaim
    {
        public int UserOperationClaimID { get; set; }
        public int UserID { get; set; }
        public int OperationClaimID { get; set; }
    }
}
namespace Comms.Domain.Models
{
    public class Lead
    {
        public string LeadID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateCreated { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Branch { get; set; }
        public List<string> PolicyNumber { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public double ArrearsAmount { get; set; }
        public DateTime ArrearsDueDate { get; set; }
        public DateTime? ArrearsSettlementDate { get; set; }
        public DateTime? LastContactDate { get; set; }
        public LeadStatus Status { get; set; }
    }
}

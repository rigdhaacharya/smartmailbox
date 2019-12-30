// ------------------------------------------------------------------------------
// <copyright file="MailItem.cs">
//   Mail item 
// </copyright>
// ------------------------------------------------------------------------------

namespace TrainMailEdge.Models
{
    public class MailItem
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string SendDate { get; set; }
        public string ReceiveDate { get; set; }
        public bool PredictedSpam { get; set; }
        public bool IsSpam { get; set; }
    }
}

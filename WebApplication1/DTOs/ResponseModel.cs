namespace WebApplication1.DTOs
{
    public class ResponseModel
    {
        public ResponseModel(bool Status, string Message, object Model)
        {
            this.Status = Status;
            this.Message = Message;
            this.Model = Model;
        }
        public ResponseModel() { }
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Model { get; set; }
    }
}

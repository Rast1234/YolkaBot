namespace YolkaBot.Client
{
    public class ActionRequest
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public bool Stop { get; set; }
        public bool Activate { get; set; }
        public bool Exit { get; set; }
    }
}
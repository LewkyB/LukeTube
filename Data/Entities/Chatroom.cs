namespace luke_site_mvc.Data
{
    public class ChatroomData
    {
        public Chatroom[] chatroom { get; set; }
    }

    public class Chatroom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }
    }
}

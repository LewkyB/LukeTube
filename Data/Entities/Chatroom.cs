using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luke_site_mvc.Data
{
    public class ChatroomData
    {
        public Chatroom[] chatroom { get; set; }
    }

    public class Chatroom
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }
    }
}

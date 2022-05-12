

using System;

namespace OneYearLater.Management.ViewModels
{
    public class ConversationalistViewModel
    {
        public int Id { get; set; }
		
        public string Hash { get; set; }
        
        public string Name { get; set; }
		
        public DateTime Created { get; set; }
        public DateTime LastEdited { get; set; }
    }
}
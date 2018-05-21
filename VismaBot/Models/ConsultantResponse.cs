using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VismaBot.Models
{

    public class ConsultantResponse
    {
        public Consultant[] consultants { get; set; }
    }

    public class Consultant
    {
        public string aboutMe { get; set; }
        public string[] askMeAbout { get; set; }
        public string[] myProjects { get; set; }
        public string[] mySkills { get; set; }
        public string name { get; set; }
    }



}
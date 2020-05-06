using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TitanicPrediction.Models
{
    public class Titanic
    {
        //public List<string> PassengerId = new List<string> { "1", "2"  }; //"892"
        public IList<SelectListItem> PassengerIdList; //"892"
        public string PassengerId; //"892"
        public string Name;
        public string Pclass; //"3"
        public string Sex; //"male"
        public string Age; //"34.5"
        public string SibSp; //"0"
        public string Parch; //"0"
        public string Fare; //"7.8292"
        public string Embarked; //"Q"
        public string Survived; //"Q"
        public string ScoredProbability; //"Q"
    }
}
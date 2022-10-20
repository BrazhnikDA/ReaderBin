using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderBin
{
    internal class ListModel
    {
        private string _id;     // id
        private string _card;   // Card
        public string Card
        {
            get { return _card; }
            set { _card = value; }
        }

        public string Id { get => _id; set => _id = value; }
    }
}

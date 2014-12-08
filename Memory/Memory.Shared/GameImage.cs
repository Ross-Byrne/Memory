using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Memory
{
    public class GameImage : INotifyPropertyChanged 
    {
        // PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public string ImageTag { get; set; }

        public string ActualImage { get; set; }

        public bool pairFound { get; set; }

        private string imageSource;
        public string ImageSource 
        {
            get
            {
                return imageSource;
            }
            set 
            {
                imageSource = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs(
                            "ImageSource"));
                } // if
            } // get/set
        } // ImageSource
    } // class
}

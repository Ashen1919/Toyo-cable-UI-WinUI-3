using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Toyo_cable_UI.Models
{
    public class Order
    {
        private bool _isReturned;
        public Guid Id { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.UtcNow;

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItems>? OrderItems { get; set; }

        public bool IsReturned
        {
            get => _isReturned;
            set
            {
                if (_isReturned != value)
                {
                    _isReturned = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusBackgroundColor));
                    OnPropertyChanged(nameof(StatusForegroundColor));
                }
            }
        }

        public string StatusText => IsReturned ? "Returned" : "Completed";

        public SolidColorBrush StatusBackgroundColor => IsReturned
            ? new SolidColorBrush(ColorHelper.FromArgb(255, 254, 226, 226)) 
            : new SolidColorBrush(ColorHelper.FromArgb(255, 231, 245, 233)); 

        public SolidColorBrush StatusForegroundColor => IsReturned
            ? new SolidColorBrush(ColorHelper.FromArgb(255, 220, 38, 38)) 
            : new SolidColorBrush(ColorHelper.FromArgb(255, 46, 125, 50)); 

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
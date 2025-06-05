using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static WpfTestCase.MainWindow;

namespace WpfTestCase
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orders;
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set { _orders = value; OnPropertyChanged(); }
        }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set { _selectedOrder = value; OnPropertyChanged(); }
        }

        public OrderViewModel()
        {
            // ตัวอย่างข้อมูล
            Orders = new ObservableCollection<Order>
            {
                new Order {
                    RunNo = "1",
                    Number = "101",
                    MM = "M01",
                    TransactionDate = DateTime.Now,
                    OrderId = "ORD1001",
                    TicketNo = "TKT1001",
                    IsSameDay = "true",
                    Delivery = "Express",
                    Status = "Completed",
                    OrderError = "None",
                    Pos = "POS1",
                    RootCause = "N/A",
                    Error = "None",
                    Job = "JOB001",
                    CaseIrNo = "IR1001",
                    User = "User1"
                },
                // เพิ่มข้อมูลอื่นๆ ตามต้องการ
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using LineDirection.Services;
using LineVector.ViewModel;

namespace lineDirection
{
    /// <summary>
    /// Interaction logic for LineDirectionMainView.xaml
    /// </summary>
    public partial class LineDirectionMainView : Window
    {
        public LineDirectionMainView()
        {
            InitializeComponent();
            this.DataContext = new MainViewViewModel(new LineDirectionService(), this);
        }
    }
}

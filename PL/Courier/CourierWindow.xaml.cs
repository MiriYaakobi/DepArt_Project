using System;
using System.Windows;
using BO;

namespace PL.Courier
{
    public partial class CourierWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // מזהה המנהל המבצע את הפעולות (קבוע כרגע ל-1)
        private const int AdminId = 1;

        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

        public CourierWindow(int? courierId = null)
        {
            InitializeComponent();

            // טעינת סוגי רכב
            cmbVehicle.ItemsSource = Enum.GetValues(typeof(BO.DeliveryType));

            if (courierId == null)
            {
                // מצב הוספה
                CurrentCourier = new BO.Courier();
            }
            else
            {
                // מצב עדכון - קריאה עם AdminId
                try
                {
                    CurrentCourier = s_bl.Courier.Read(AdminId, (int)courierId);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Error");
                    Close();
                }
            }
        }

        private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // נסיון בדיקה האם השליח קיים
                bool exists = false;
                if (CurrentCourier.Id != 0)
                {
                    try
                    {
                        s_bl.Courier.Read(AdminId, CurrentCourier.Id);
                        exists = true;
                    }
                    catch { exists = false; }
                }

                if (exists)
                {
                    // עדכון (שליחת ID מנהל + האובייקט)
                    s_bl.Courier.Update(AdminId, CurrentCourier);
                    CustomMessageBox.Show("Courier updated successfully!", "Success");
                }
                else
                {
                    // הוספה (שליחת ID מנהל + האובייקט)
                    s_bl.Courier.Create(AdminId, CurrentCourier);
                    CustomMessageBox.Show("Courier added successfully!", "Success");
                }

                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Operation failed: {ex.Message}", "Error");
            }
        }
    }
}
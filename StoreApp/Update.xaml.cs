using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace StoreApp
{
    public partial class Update : Window
    {
        public MainWindow mainWindow = (MainWindow)Application.Current.MainWindow; //Access MainWindow by Owner

        //public MainWindow mainWindow => (MainWindow)Owner;  //Access MainWindow by Owner

        public string filename = @"employee.json";
        public FileInfo TempImageFile { get; set; }         //Using for upload image
        public FileInfo OldImageFile { get; set; }          //Using for exists image
        public Update()
        {
            InitializeComponent();
            string[] titles = new string[] { "Mr", "Miss", "Mrs" };
            this.cmbTitle.ItemsSource = titles;
            cmbTitle.SelectedIndex = -1;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var Id = Convert.ToInt32(txtId.Text);
            var Title = cmbTitle.SelectedItem.ToString();
            var FirstName = txtFirstName.Text;
            var LastName = txtLastName.Text;
            var Email = txtEmail.Text;
            var Contact = txtContactNo.Text;

            var json = File.ReadAllText(filename);
            var jsonObj = JObject.Parse(json);
            var empJson = jsonObj.GetValue("Employees").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);

            foreach (var item in empList.Where(x => x.Id == Id))
            {
                item.Title = Title;
                item.FirstName = FirstName;
                item.LastName = LastName;
                item.Email = Email;
                item.Contact = Contact;
                OldImageFile = (item.ImageTitle != "default.png") ? new FileInfo(mainWindow.GetImagePath() + item.ImageTitle) : null;   //ternary to evaluate null if exists image is default image

                if (TempImageFile != null && OldImageFile == null)  //Check if upload image not null && exists image is null or default.png
                {
                    TempImageFile.CopyTo(mainWindow.GetImagePath() + item.Id + TempImageFile.Extension);
                    item.ImageTitle = item.Id + TempImageFile.Extension;
                    TempImageFile = null;
                }
                if (OldImageFile != null && TempImageFile != null) //Check if upload image not null && old image not null. Extra -> check if old file exists in directory
                {
                    item.ImageTitle = item.Id + TempImageFile.Extension;
                    OldImageFile.Delete();      //Delete exists image
                    TempImageFile.CopyTo(mainWindow.GetImagePath() + Id + TempImageFile.Extension); //Copy upload image to target directory
                    TempImageFile = null;
                }

            }

            var empArray = JArray.FromObject(empList);  //Convert List<Emoloyee> to Jarray
            jsonObj["Employees"] = empArray;            //Set Jarray to 'Employees' JProperty
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);  //Serialize data using Extension Method
            File.WriteAllText(filename, output);

            this.Close();                               //Close the current window
            mainWindow.ShowData();                      //Call Mainwindow ShowData() Method
            mainWindow.ShowWindow();
            MessageBox.Show("Data Updated Successfully !!");

        }

        private void btnImgModify_Click(object sender, RoutedEventArgs e)   //Upload Image
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image Files(*.jpg; *.jpeg; *.png;)|*.jpg; *.jpeg; *.png;";
            fd.Title = "Select an Image";
            if (fd.ShowDialog().Value == true)
            {
                ImgModify.Source = mainWindow.ImageInstance(new Uri(fd.FileName));  //ImageInstance return new instance of image rather than image reference
                TempImageFile = new FileInfo(fd.FileName);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainWindow.Show();
        }
    }
}

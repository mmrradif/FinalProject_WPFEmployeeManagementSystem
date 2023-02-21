using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StoreApp
{
    public partial class MainWindow : Window
    {
        public string filename = @"employee.json";
        public FileInfo TempImageFile { get; set; }
        public BitmapImage DefaultImage => new BitmapImage(new Uri(GetImagePath() + "default.png"));

        public MainWindow()
        {
            InitializeComponent();
            string[] titles = new string[] { "Mr", "Miss", "Mrs" };
            this.cmbTitle.ItemsSource = titles;
            cmbTitle.SelectedIndex = -1;

            var path = Path.GetDirectoryName(GetImagePath());
            if (!File.Exists(filename))
            {
                File.CreateText(filename).Close();
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ImgDisplay.Source = DefaultImage;
            ShowData();

        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {

            Employee em = new Employee()
            {
                ImageTitle = (TempImageFile != null) ? $"{int.Parse(txtId.Text) + TempImageFile.Extension}" : "default.png",
                Id = int.Parse(txtId.Text),
                Title = cmbTitle.SelectedItem.ToString(),
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Email = txtEmail.Text,
                Contact = txtContactNo.Text
            };

            string filedata = File.ReadAllText(filename);
            if (IsValidJson(filedata) && IsExists("Employees") && !IsIdExists(em.Id)) //check file contains valid json format and exists "Employees" Parent Node
            {
                var data = JObject.Parse(filedata);
                var empJson = data.GetValue("Employees").ToString();
                var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);
                empList.Add(em);
                JArray empArray = JArray.FromObject(empList);
                data["Employees"] = empArray;
                var newJsonResult = JsonConvert.SerializeObject(data, Formatting.Indented);

                if (TempImageFile != null)
                {
                    TempImageFile.CopyTo(GetImagePath() + em.ImageTitle);
                    TempImageFile = null;
                    ImgDisplay.Source = DefaultImage;
                }
                File.WriteAllText(filename, newJsonResult);     //write all employees to json file
            }

            if (!IsValidJson(filedata))
            {
                var emp = new { Employees = new Employee[] { em } };  //create json format with parent[Employees]
                string newJsonResult = JsonConvert.SerializeObject(emp, Formatting.Indented);   //serialize json format
                if (TempImageFile != null)
                {
                    TempImageFile.CopyTo(GetImagePath() + em.ImageTitle);
                    TempImageFile = null;
                    ImgDisplay.Source = DefaultImage;
                }
                File.WriteAllText(filename, newJsonResult);         //write json format to employee.json
            }
            ShowData();

        }
        private bool IsIdExists(int inputId)    //input id from input box
        {
            string filedata = File.ReadAllText(filename);
            var data = JObject.Parse((string)filedata);              //parse file data as JObject
            var empJson = data.GetValue("Employees").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);

            var exists = empList.Find(x => x.Id == inputId);                 //return employee if id found, else return null

            if (exists != null)
            {
                MessageBox.Show($"ID - {exists.Id} exists\nTry with different Id", "Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            else
            {
                return false;
            }

        }

        private bool IsValidJson(string data)   //check whether file contains json format or not
        {

            try
            {
                var temp = JObject.Parse(data);  //Try to parse json data if can't will throw exception
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsExists(string data)      //Check if exists parent node ('Employees') in json file
        {
            string filedata = File.ReadAllText(filename);
            var jsonObject = JObject.Parse(filedata);
            var empJson = jsonObject[data];     //If not exists return null

            return (empJson != null) ? true : false;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Update update = new Update();
            update.Owner = this;

            Button b = sender as Button;
            Employee empbtn = b.CommandParameter as Employee;

            update.txtId.IsEnabled = false;
            update.txtId.Text = empbtn.Id.ToString();
            update.cmbTitle.Text = empbtn.Title;
            update.txtFirstName.Text = empbtn.FirstName;
            update.txtLastName.Text = empbtn.LastName;
            update.txtEmail.Text = empbtn.Email;
            update.txtContactNo.Text = empbtn.Contact;
            update.ImgModify.Source = empbtn.ImageSrc;
            this.Hide();
            update.Show();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var jsonD = File.ReadAllText(filename);
            var jsonObj = JObject.Parse(jsonD);
            var empJson = jsonObj.GetValue("Employees").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);

            Button b = sender as Button;
            Employee empbtn = b.CommandParameter as Employee;
            int empId = empbtn.Id;

            MessageBoxResult result = MessageBox.Show($"Are you want to delete ID - {empId}", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes) //if press 'Yes' on delete confirmation
            {
                empList.Remove(empList.Find(x => x.Id == empId));   //Remove the employee from the list
                JArray empArray = JArray.FromObject(empList);       //Convert List<Employee> to JArray
                jsonObj["Employees"] = empArray;                    //Add JArray to 'Employees' JProperty
                var newJsonResult = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

                FileInfo thisFile = new FileInfo(GetImagePath() + empbtn.ImageTitle);
                if (thisFile.Name != "default.png") //Delete image (Not default image)
                {
                    thisFile.Delete();
                }

                File.WriteAllText(filename, newJsonResult);

                MessageBox.Show("Data Deleted Successfully !!", "Delete", MessageBoxButton.OK, MessageBoxImage.Question);
                ShowData();
                AllClear();
            }
            else
            {
                return;
            }
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            ShowData();
        }
        public void ShowData()
        {
            var json = File.ReadAllText(filename);

            if (!IsValidJson(json))
            {
                return;
            }

            var jsonObj = JObject.Parse(json);
            var empJson = jsonObj.GetValue("Employees").ToString();
            var empList = JsonConvert.DeserializeObject<List<Employee>>(empJson);   //Deserialize to List<Employee>
            empList = empList.OrderBy(x => x.Id).ToList();  //Sorting List<Employee> by Id (Ascending)

            foreach (var item in empList)
            {
                item.ImageSrc = ImageInstance(new Uri(GetImagePath() + item.ImageTitle));   //Create image instance for all Employee
            }
            lstEmployee.ItemsSource = empList;
            lstEmployee.Items.Refresh();

            GC.Collect();                   //Call garbage collector to release unused image instance resource
            GC.WaitForPendingFinalizers();
        }
        public ImageSource ImageInstance(Uri path)  //Create image instance rather than referencing image file
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = path;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.DecodePixelWidth = 300;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        public string GetImagePath()    //Get the Image Directory Path Where Image is stored
        {
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyDirectory = Path.GetDirectoryName(currentAssembly.Location);             // debug folder
            string ImagePath = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\Img\"));    // ..\..\ Navigate two levels up => Project folder

            return ImagePath;
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image Files(*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png;";
            fd.Title = "Select an Image";
            if (fd.ShowDialog().Value == true)
            {
                ImgDisplay.Source = new BitmapImage(new Uri(fd.FileName));
                TempImageFile = new FileInfo(fd.FileName);
            }
        }
        public void AllClear()
        {
            txtId.Clear();
            cmbTitle.SelectedIndex = -1;
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtContactNo.Clear();
            txtId.IsEnabled = true;
        }
        public void ShowWindow()
        {
            this.Show();
        }
    }
}

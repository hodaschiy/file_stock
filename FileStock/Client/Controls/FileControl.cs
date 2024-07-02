using Newtonsoft.Json;
using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Shapes;

namespace Client.Controls
{
    public class FileControl
    {
        public List<FileModel> GetUserFiles()
        {
            return App.http.GetFromJsonAsAsyncEnumerable<FileModel>($"/FileModels/Get?Login={App.usr.Name}&Token={App.usr.Token}").ToListAsync<FileModel>().Result;
        }
        public async Task<FileModel> Add() 
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false; // явное указание, что выбирается только 1 файл, потом можно доработать на мультивыбор

            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                JsonContent jsonContent = JsonContent.Create(new { Login = App.usr.Name, Token = App.usr.Token, Name = dlg.SafeFileName, Data = File.ReadAllBytes(dlg.FileName) });

                var res = await App.http.PostAsync("/FileModels/Add", jsonContent);
                var fl = await res.Content.ReadFromJsonAsync<FileModel>();
                return fl;
            }
             else
            {
                return null;
            }
        }
        public async Task<bool> Remove(int id)
        {
            JsonContent jsonContent = JsonContent.Create(new { Login = App.usr.Name, Token = App.usr.Token, Id = id});
            var res = await App.http.PostAsync("/FileModels/Delete", jsonContent); 
            var IsDeleted = await res.Content.ReadFromJsonAsync<bool>();
            return IsDeleted;
        }
        public async Task<bool> Download(FileModel savingFile)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            string path = dialog.SelectedPath;

            JsonContent jsonContent = JsonContent.Create(new { Login = App.usr.Name, Token = App.usr.Token, Id = savingFile.Id });
            var res = await App.http.PostAsync("/FileModels/Download", jsonContent);
            var stream = await res.Content.ReadAsStreamAsync();
            FileStream fl = File.Create(path+'/'+savingFile.Name);
            stream.CopyTo(fl);
            stream.Close();
            fl.Close();

            return true;
        }
    }
}

using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics;

namespace Myshop.ViewModel
{
    public class EditCategoryViewModel : ViewModelBase
    {
        public ICommand UpdateInfoCommand { get; }
        public ICommand AddCategoryCommand { get; }
        
        private readonly BookViewModel model;
        private readonly ObservableCollection<Category> categories;

        private List<Category> originalCat;

        private CollectionViewSource CatItemsCollection = new CollectionViewSource { };
        public ICollectionView CatSourceCollection => CatItemsCollection.View;

        public EditCategoryViewModel()
        {

        }

        public EditCategoryViewModel(BookViewModel model, ObservableCollection<Category> cats)
        {
            this.model = model;
            this.categories = new ObservableCollection<Category>(cats.Skip(1).Take(cats.Count - 2));
            this.originalCat = new List<Category>();
            foreach(var c in cats.Skip(1).Take(cats.Count - 2).ToList())
            {
                originalCat.Add(new Category() { Name=c.Name, Id=c.Id});
            }
            CatItemsCollection = new CollectionViewSource { Source = categories };
            OnPropertyChanged(nameof(CatSourceCollection));
            UpdateInfoCommand = new ViewModelCommand(ExecuteUpdate);
            AddCategoryCommand = new ViewModelCommand(ExecuteAddCategory);
        }



        public async Task SendPutRequestAsyncForCat(string request)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].Id == 0)
                {
                    // post cat
                    var json = new JsonObject
                    {
                        {"categoryName",categories[i].Name.Trim()}
                    };
                    var content = new StringContent(JsonSerializer.Serialize(json), Encoding.Default, "application/json");

                    using var httpClient = new HttpClient();
                    using (var response = await httpClient.PostAsync(request, content))
                    {
                        Debug.WriteLine(response.Content);
                        response.EnsureSuccessStatusCode();
                    }
                }
                else
                {
                    if (categories[i].Name.Trim() != originalCat[i].Name.Trim())
                    {
                        // put cat
                        request += "/" + categories[i].Id;
                        var json = new JsonObject
                        {
                            {"categoryName",categories[i].Name.Trim()}
                        };
                        var content = new StringContent(JsonSerializer.Serialize(json), Encoding.Default, "application/json");

                        using var httpClient = new HttpClient();
                        using (var response = await httpClient.PutAsync(request, content))
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }

            }
        }

        public async void ExecuteUpdate(object obj)
        {
            CatItemsCollection = new CollectionViewSource { Source = categories };
            OnPropertyChanged(nameof(CatSourceCollection));
            model.UpdateCategoryList(new ObservableCollection<Category>(categories.Where(x => x.Name.Length != 0)));
            await SendPutRequestAsyncForCat("https://hcmusshop.azurewebsites.net/api/Category");
        }

        public void ExecuteAddCategory(object obj)
        {
            categories.Add(new Category() { Id = 0});
            CatItemsCollection = new CollectionViewSource { Source = categories };
            OnPropertyChanged(nameof(CatSourceCollection));
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PM2IIPExamen.Controller;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using Android.Media;

namespace PM2IIPExamen
{
  
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class lista : ContentPage
    {
        
        public lista()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ListaEmpleados.ItemsSource = await sitiosController.ObtenerSitios();
        }

        private async void ListaEmpleados_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

            String sexResult = await DisplayActionSheet("Seleccione una opción ", "Cancelar", null, "Actualizar", "Mapa", "Eliminar", "Reproducir");
            var d = e.SelectedItem as Models.apiSitios.SitioC;

            if (sexResult == "Reproducir")
            {
                MediaPlayer mediaPlayer = new MediaPlayer();
                mediaPlayer.SetDataSource(new Controller.Reproductor(Convert.FromBase64String(d.audio)));
                //mediaPlayer.Reset();
                mediaPlayer.Prepare();
                mediaPlayer.Start();    
            }

            if (sexResult == "Eliminar")
            {

                String direccion = "https://examen2ppm01.000webhostapp.com/api/delete/contacto.php?id=" + d.id;
                StringContent id = new StringContent(d.id);
                

                using (HttpClient client = new HttpClient())
                {
                    var respuesta = await client.GetAsync(direccion);

                    Debug.WriteLine(respuesta.Content.ReadAsStringAsync().Result);
                    await DisplayAlert("¡Notificación!", "Datos Eliminados", "OK");
                }

            }

            if (sexResult == "Actualizar")
            {
               
                var newpage = new Editar(d);
                Debug.WriteLine(d.foto);
                await Navigation.PushAsync(newpage);
            }

            if (sexResult == "Mapa")
            {
                var location = new Location(Double.Parse(d.latitud),Double.Parse(d.longitud));
                var options = new MapLaunchOptions { NavigationMode = NavigationMode.Driving };

                await Map.OpenAsync(location, options);
                // var newpage = new mapa( d.latitud, d.longitud, d.descripcion);
                // await Navigation.PushAsync(newpage);
            }

                /* audio*/
                String b64 = d.audio;
            
        }

    }
}
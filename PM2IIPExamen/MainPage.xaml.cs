using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using Plugin.Media;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Plugin.Geolocator;
using Plugin.AudioRecorder;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Net.Http;
using Plugin.Media.Abstractions;
using Android.Graphics;
using Newtonsoft.Json.Linq;
using Android.App.Job;

namespace PM2IIPExamen
{
    public partial class MainPage : ContentPage
    {   // variables
        
        string ruta = "", StringBase64Foto = "", StringBase64Audio= "" ,StringBase64Video = ""; //ruta de la imagen
        int aud = 0; // validaciones
        AudioRecorderService recorder = new AudioRecorderService();// grabadora


        public MainPage()
        {
            InitializeComponent();

            InizializatePlugins();
        }

        Plugin.Media.Abstractions.MediaFile photo = null;
        private String traeImagenToBase64()
        {
            if (photo != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    Stream stream = photo.GetStream();
                    stream.CopyTo(memory);
                    byte[] fotobyte = memory.ToArray();

                   byte[] imagenescalada = obtener_imagen_escalada(fotobyte,50);

                   // string Base64String = Convert.ToBase64String(fotobyte);
                   string Base64String = Convert.ToBase64String(imagenescalada);
                    return Base64String;
                }
            }
            return null;

        }
        private byte [] obtener_imagen_escalada(byte[]imagen,int compresion)
        {
            Bitmap original  = BitmapFactory.DecodeByteArray(imagen,0,imagen.Length);
            using (MemoryStream memory = new MemoryStream())
            {
                original.Compress(Bitmap.CompressFormat.Jpeg,compresion,memory);
                return memory.ToArray();
            }

        }
        private async void InizializatePlugins()
        {

            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    lblLatitud.Text = location.Latitude.ToString();
                    lblLongitud.Text = location.Longitude.ToString();
                }
                else{
                    await DisplayAlert("¡ALERTA!","ACTIVAR GPS", "OK");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }

           

        }

         bool verificarInternet()
        {
           
                var current = Connectivity.NetworkAccess;

            return current != NetworkAccess.Internet;

        }

   

        private async void guardar()
        {

            if (String.IsNullOrWhiteSpace(txtDescripcion.Text))
            {

                await DisplayAlert("Error", "No completó todos los campos", "OK");
            }
            else if(verificarInternet())
            {
                await DisplayAlert("¡AVISO!", "NO TIENE ACCESO A INTERNET, POR FAVOR ACTIVARLO", "OK");
            }
            else
            {

                String direccion = "https://examen2ppm01.000webhostapp.com/api/post/contacto.php";
                var json = new JObject();
                json.Add("descripcion", txtDescripcion.Text);
                json.Add("longitud", lblLongitud.Text);
                json.Add("latitud", lblLatitud.Text);
                json.Add("audio", traeraudioToBase64());
                json.Add("foto", traeImagenToBase64());
                Debug.WriteLine(traeraudioToBase64());

                var conten = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    var respuesta = await client.PostAsync(direccion, conten);

                    Debug.WriteLine(respuesta.Content.ReadAsStringAsync().Result);
                    if(respuesta.IsSuccessStatusCode)
                    {
                        await DisplayAlert("¡Notificación!", "Datos Guardados", "OK");

                        reset();
                    }
                    
                  
                }                
            }

        }

        private async void btnFoto_Clicked(object sender, EventArgs e)
        {

           

            if (string.IsNullOrEmpty(txtDescripcion.Text))
             {
                 await DisplayAlert("Alerta", "Antes de Tomar el Video Debe Llenar los Campos Vacios", "Ok");
             }
             else
             {
                photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "FotosAplicacion",
                    Name = "PhotoAlbum.jpg",
                    SaveToAlbum = true
                });
                if (photo != null)
                {
                    Foto.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                }
            }

        }

        private void reset()
        {
            txtDescripcion.Text = "";
            ruta = "";
            imgFoto.Source = "paisajes.gif";
            aud = 0;
            lblAudio.Text = "Estatus Audio";
            lblAudio.TextColor = System.Drawing.Color.Blue;
            recorder = new AudioRecorderService();
        }

        private async void btnGrabar_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!recorder.IsRecording)
                {
                    recorder = new AudioRecorderService();

                    lblAudio.Text = "Grabando";
                    await recorder.StartRecording();

                }
            }
           catch(Exception)
            {
                lblAudio.Text = "Estatus Audio";
                await DisplayAlert("¡ALERTA!", "ACTIVAR PERIMISOS DE MICROFONO", "OK");
            }


        }

        private async void btnDetener_Clicked(object sender, EventArgs e)
        {
            if (recorder.IsRecording)
            {
                lblAudio.Text = "Audio Detenido";
                await recorder.StopRecording();
                aud = 1;
 
            }

        }

        private string traeraudioToBase64()
        {
            byte[] AudioBytes = null;

            using (var stream = new MemoryStream())
            {
                recorder.GetAudioFileStream().CopyTo(stream);
                AudioBytes = stream.ToArray();
                return Convert.ToBase64String(AudioBytes);
            }
        }

        private void btnReproducir_Clicked(object sender, EventArgs e)
        {
            AudioPlayer player = new AudioPlayer();
            var filePath = recorder.GetAudioFilePath();
            lblAudio.Text = "Reproduciendo";
            player.Play(filePath);

            lblAudio.Text = "Sin acción";
        }

        private void btnGuardar_Clicked(object sender, EventArgs e)
        {
            guardar();
        }

        private async void btnListar_Clicked(object sender, EventArgs e)
        {
           
            if (verificarInternet())
            {
                await DisplayAlert("¡AVISO!", "NO TIENE ACCESO A INTERNET, POR FAVOR ACTIVARLO", "OK");
                return;
            }
            var am = new lista();
            await Navigation.PushAsync(am);
        }
    }
}

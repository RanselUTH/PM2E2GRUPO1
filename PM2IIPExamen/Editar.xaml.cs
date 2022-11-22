using Android.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM2IIPExamen.Models;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using Plugin.AudioRecorder;
using Android.Util;
using Plugin.Media;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Xamarin.Essentials;

namespace PM2IIPExamen
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Editar : ContentPage
    {
        string ruta = "";
        AudioRecorderService recorder = new AudioRecorderService();// grabadora
        int aud = 0; // validaciones
        apiSitios.SitioC sitio;

        public Editar(apiSitios.SitioC s)
        {
            InitializeComponent();
            sitio = s;
            Foto.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(sitio.foto)));
            lblLatitud.Text = sitio.latitud;
            lblLongitud.Text = sitio.longitud;
            txtDescripcion.Text = sitio.descripcion;


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

                    byte[] imagenescalada = obtener_imagen_escalada(fotobyte, 50);

                    // string Base64String = Convert.ToBase64String(fotobyte);
                    string Base64String = Convert.ToBase64String(imagenescalada);
                    return Base64String;
                }
            }
            return null;

        }
        private byte[] obtener_imagen_escalada(byte[] imagen, int compresion)
        {
            Bitmap original = BitmapFactory.DecodeByteArray(imagen, 0, imagen.Length);
            using (MemoryStream memory = new MemoryStream())
            {
                original.Compress(Bitmap.CompressFormat.Jpeg, compresion, memory);
                return memory.ToArray();
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

        private async void btnGrabar_Clicked(object sender, EventArgs e)
        {
            if (!recorder.IsRecording)
            {
                recorder = new AudioRecorderService();

                lblAudio.Text = "Grabando";
                await recorder.StartRecording();

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


        bool verificarInternet()
        {

            var current = Connectivity.NetworkAccess;

            return current != NetworkAccess.Internet;

        }


        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtDescripcion.Text))
            {

                await DisplayAlert("Error", "No se relleno todos los campos", "OK");
            }
            else if (verificarInternet())
            {
                await DisplayAlert("¡AVISO!", "NO TIENE ACCESO A INTERNET, POR FAVOR ACTIVARLO", "OK");
                return;
            }
            else
            {


                String direccion = "https://examen2ppm01.000webhostapp.com/api/put/contacto.php";
                var json = new JObject();
                json.Add("id",sitio.id);
                json.Add("descripcion", txtDescripcion.Text);
                json.Add("longitud", lblLongitud.Text);
                json.Add("latitud", lblLatitud.Text);
                var audio = traeImagenToBase64();
                if(audio == null)
                {
                    audio = sitio.audio;
                }
                json.Add("audio", audio);
                json.Add("foto", traeImagenToBase64());

                var conten = new StringContent(json.ToString(), Encoding.UTF8, "application/json");



                using (HttpClient client = new HttpClient())
                {
                    var respuesta = await client.PostAsync(direccion, conten);

              
                    if (respuesta.IsSuccessStatusCode)
                    {
                        await DisplayAlert("¡Notificación!", "Datos Actualiazados", "OK");

                        reset();
                    }


                }
            }
        }

       

        private void btnReproducirNuevo_Clicked(object sender, EventArgs e)
        {
            AudioPlayer player = new AudioPlayer();
            var filePath = recorder.GetAudioFilePath();
            lblAudio.Text = "Reproduciendo";
            player.Play(filePath);

            lblAudio.Text = "Sin acción";
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

    }
}
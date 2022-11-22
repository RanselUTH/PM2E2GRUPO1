using Android.Graphics;
using Newtonsoft.Json.Linq;
using Plugin.AudioRecorder;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PM2IIPExamen
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class vista : ContentPage
    {
        AudioRecorderService recorder = new AudioRecorderService();// grabadora
        String ide, des, lat, lon, img, audio, StringBase64Foto = "", StringBase64Audio = "", StringBase64Video = "", ruta ="";


        public vista(String id_, String des_, String lat_, String lon_, String img_, String audio_)
        {
            InitializeComponent();
            InizializatePlugins();
            var byteArray = Convert.FromBase64String(img_);
            System.IO.Stream stream = new MemoryStream(byteArray);
            var imageSource = ImageSource.FromStream(() => stream);
            imgFoto.Source = imageSource;

            ide = id_;
            des = des_;
            lat = lat_;
            lon = lon_;
            audio = audio_;
            img = img_;


            txtDescripcion.Text = des;
            lblLatitud.Text = lat_;
            lblLongitud.Text = lon_;

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

        private void btnNuevo_Clicked(object sender, EventArgs e)
        {
            InizializatePlugins();
        }

        int aud = 0;
        private async void  btnActualizar_Clicked(object sender, EventArgs e)
        {
            String  desA="", latA="", lonA="", imgA="", audioA="";
            if (!String.IsNullOrWhiteSpace(txtDescripcion.Text)) { 
                    if (!txtDescripcion.Text.Equals(des))
                    {
                        desA=txtDescripcion.Text;
                    }
                    else { desA = des; }

                    if (!lblLatitud.Text.Equals(lat))
                    {
                        latA = lblLatitud.Text;
                    }
                    else
                    {
                        latA = lat;
                    }

                    if (!lblLongitud.Text.Equals(lon))
                    {
                        lonA = lblLongitud.Text;
                    }
                    else
                    {
                        lonA = lon;
                    }

                    if (StringBase64Foto.Equals(""))
                    {
                        
                        imgA = img;
                    }
                    else
                    {
                        imgA = StringBase64Foto;
                    }

                    if (StringBase64Audio.Equals(""))
                    {
                        audioA = audio;
                    
                    }
                    else
                    {
                     audioA = StringBase64Audio;
                    }


                    enviar(ide,desA, lonA, latA, imgA, audioA);


            }
            else
            {
                await DisplayAlert("Error", "No completó todos los campos", "OK");
            }


        }

        private async void enviar(String a, String b, String c, String d, String e, String f)
        {
            var json = new JObject();
            json.Add("id", a);
            json.Add("descripcion", b);
            json.Add("longitud", c);
            json.Add("latitud", d);
            json.Add("audio", f);
            json.Add("foto", e);

            var conten = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

           /* MultipartFormDataContent parametros = new MultipartFormDataContent();
            StringContent id = new StringContent(a);
            StringContent dese = new StringContent(b);
            StringContent lone = new StringContent(c);
            StringContent late = new StringContent(d);
            StringContent imge = new StringContent(e);
            StringContent aude = new StringContent(f);
            parametros.Add(id, "id");
            parametros.Add(dese, "descripcion");
            parametros.Add(lone, "longitud");
            parametros.Add(late, "latitud");
            parametros.Add(imge, "imgT");
            parametros.Add(aude, "audioT");
           */
            String direccion = "https://examen2ppm01.000webhostapp.com/api/put/contacto.php";
            using (HttpClient client = new HttpClient())
            {
                var respuesta = await client.PostAsync(direccion, conten);

                Debug.WriteLine(respuesta.Content.ReadAsStringAsync().Result);
                await DisplayAlert("Proceso Terminado", "Datos Actualizados", "OK");

                var am = new lista();
                await Navigation.PushAsync(am);

            }
        }

        private async void InizializatePlugins()
        {

            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    // Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    lblLatitud.Text = location.Latitude.ToString();
                    lblLongitud.Text = location.Longitude.ToString();
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


        private async void tomar()
        {
            var takepic = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "PhotoApp",
                Name = "TEST.jpg"
            });


            ruta = takepic.Path;


            if (takepic != null)
            {
                imgFoto.Source = ImageSource.FromStream(() => { return takepic.GetStream(); });

            }
            byte[] ImageBytes = null;

            using (var stream = new MemoryStream())
            {
                takepic.GetStream().CopyTo(stream);
                takepic.Dispose();
                ImageBytes = stream.ToArray();
                StringBase64Foto = Convert.ToBase64String(ImageBytes);
            }
        }
        private async void btnGrabar_Clicked(object sender, EventArgs e)
        {
            if (!recorder.IsRecording)
            {
                //recorder = new AudioRecorderService();

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


                byte[] AudioBytes = null;

                using (var stream = new MemoryStream())
                {
                    recorder.GetAudioFileStream().CopyTo(stream);
                    AudioBytes = stream.ToArray();
                    StringBase64Audio = Convert.ToBase64String(AudioBytes);

                }

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
                    //Foto.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                }

                //var cameraOptions = new StoreVideoOptions();
                //cameraOptions.PhotoSize = PhotoSize.Full;
                //cameraOptions.Name = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "jpg";
                //cameraOptions.Directory = "DefaultVideos";

                //var mediaFile = await CrossMedia.Current.TakeVideoAsync(cameraOptions);

                //ruta = mediaFile.Path;
                //if (mediaFile != null)
                //{
                //    byte[] arreglo = null;
                //    {
                //        ruta = mediaFile.Path;

                //        arreglo = File.ReadAllBytes(ruta);
                //        StringBase64Video = Convert.ToBase64String(arreglo);
                //    };

                //    using (var stream = new MemoryStream())
                //    {

                //        mediaFile.GetStream().CopyTo(stream);
                //        mediaFile.Dispose();
                //        arreglo = stream.ToArray();
                //        StringBase64Video = Convert.ToBase64String(arreglo);
                //    }


                //}



            }

        }
    }
}
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Firebase;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Core.Platforms.Android;

namespace PencaMaui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        // Datos del proyecto Firebase "tupencauyproject" (google-services.json)
        const string FirebaseAppId = "1:820549641519:android:5f065e595770df33ec2dda";
        const string FirebaseApiKey = "AIzaSyB5eshB4i7G-_8cZPYCfhMbeGeK5yCaG38";
        const string FirebaseProjectId = "tupencauyproject";
        const string FirebaseGcmSenderId = "820549641519";
        const string FirebaseStorageBucket = "tupencauyproject.firebasestorage.app";

        const string NotificationChannelId = "pencamaui.general";

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var firebaseOptions = new FirebaseOptions.Builder()
                .SetApplicationId(FirebaseAppId)
                .SetApiKey(FirebaseApiKey)
                .SetProjectId(FirebaseProjectId)
                .SetGcmSenderId(FirebaseGcmSenderId)
                .SetStorageBucket(FirebaseStorageBucket)
                .Build();

            CrossFirebase.Initialize(this, firebaseOptions);

            CrearCanalDeNotificaciones();
            PedirPermisoDeNotificacionesSiHaceFalta();
        }

        void CrearCanalDeNotificaciones()
        {
            // Los canales de notificación recién existen desde Android 8 (API 26)
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
                var channel = new NotificationChannel(NotificationChannelId, "General", NotificationImportance.Default);
                notificationManager.CreateNotificationChannel(channel);
            }

            FirebaseCloudMessagingImplementation.ChannelId = NotificationChannelId;
        }

        void PedirPermisoDeNotificacionesSiHaceFalta()
        {
            // POST_NOTIFICATIONS solo se pide en runtime a partir de Android 13 (API 33)
            if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
                return;

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                ActivityCompat.RequestPermissions(this, new[] { Android.Manifest.Permission.PostNotifications }, 0);
        }
    }
}

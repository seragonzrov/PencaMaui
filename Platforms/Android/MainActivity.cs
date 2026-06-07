using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Firebase;
using Plugin.Firebase.Auth.Google;
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
        const string GoogleWebClientId = "820549641519-jctbpa8906f4ed4a561lhrsp2mr1eoq1.apps.googleusercontent.com";

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
            FirebaseAuthGoogleImplementation.Initialize(GoogleWebClientId);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            FirebaseAuthGoogleImplementation.HandleActivityResultAsync(requestCode, resultCode, data);
        }
    }
}

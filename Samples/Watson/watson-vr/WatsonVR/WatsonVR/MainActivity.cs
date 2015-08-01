﻿using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Android.Database;
using Uri = Android.Net.Uri;
namespace WatsonVR
{
	[Activity (Label = "WatsonVR", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;
		public static readonly int PickImageId = 1000;
		private ImageView _imageView;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			TextView result = FindViewById<TextView> (Resource.Id.textView1);
			button.Click += delegate {
				/*Intent = new Intent();
				Intent.SetType("image/*");
				Intent.SetAction(Intent.ActionGetContent);
				StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
*/
				PostAsync();
				result.Text = string.Format ("{0} clicks!", count++);
			};
		}
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
			{
				Uri uri =  data.Data;
				//_imageView.SetImageURI(uri);

				string path = GetPathToImage(uri);
				//PostAsync (uri.ToString());
			}
		}
		private string GetPathToImage(Uri uri)
		{
			string path = null;
			// The projection contains the columns we want to return in our query.
			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
			using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
			{
				if (cursor != null)
				{
					int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
					cursor.MoveToFirst();
					path = cursor.GetString(columnIndex);
				}
			}
			return path;
		}
		public async Task<string> PostAsync ()
		{
			string url = "https://gateway.watsonplatform.net/visual-recognition-beta/api";
			try{
				using (var client = new HttpClient ()) {
					client.DefaultRequestHeaders.Accept.Clear ();
					client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("multipart/form-data"));
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Basic", GetBase64CredentialString ());
					//var response = await client.PostAsync (url, new StringContent (postData, Encoding.UTF8, "application/json")).ConfigureAwait (false);
					//return await HandleResponseAsync (response);

					string documentsPath = Android.OS.Environment.ExternalStorageDirectory.Path+"/"+Android.OS.Environment.DirectoryDownloads;
					string fileName = "1.jpeg";
					string localPath = System.IO.Path.Combine (documentsPath, fileName);

					var content = new MultipartFormDataContent();
					var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
					var streamContent = new StreamContent(fileStream);
					streamContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse("form-data");
					//					

					content.Add(streamContent, fileName, "image/jpeg");
					var response = await client.PostAsync (url, content);
					return await HandleResponseAsync (response);
				}
			}
			catch(Exception ex)
			{
				return ex.Message;
			}

		}


		async Task<string> HandleResponseAsync (HttpResponseMessage response)
		{
			string bb = await response.Content.ReadAsStringAsync ().ConfigureAwait (false);
			return bb;
		}

		private string GetCredentialString ()
		{
			return string.Format ("{0} {1}", "Basic", GetBase64CredentialString ());
		}

		private string GetBase64CredentialString ()
		{

			var auth = string.Format ("{0}:{1}", "11c82e2a-4978-4a3b-a2c1-6750cb81cbde", "T02f3gtyIAaE");
			return Convert.ToBase64String (Encoding.UTF8.GetBytes (auth));
		}
	}
}



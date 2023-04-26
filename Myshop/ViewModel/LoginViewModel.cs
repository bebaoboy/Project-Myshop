using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Configuration;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using Myshop.View;
using System.Text.Json.Serialization;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Myshop.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields
        private string _username = "";
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        private SecureString _password = new SecureString();
        public SecureString Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        private string _errorMessage;
        private bool _isViewVisible = true;

        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }

            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }
        private bool _rememberMe = false;
        private bool _oldRememberMe = false;
        private bool _noSavedUsername = false;

        public bool RememberMe
        {
            get
            {
                return _rememberMe;
            }

            set
            {
                _rememberMe = value;
                OnPropertyChanged(nameof(RememberMe));
            }
        }

        //-> Commands
        public ICommand LoginCommand { get; }
        public ICommand CheckCommand { get; }

        private string username = "";
        private string passwordIn64 = "";
        private string entropyIn64 = "";

        //Constructor
        public LoginViewModel()
        {
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            CheckCommand = new ViewModelCommand(ExecuteRememberPassowrdCommand);
            var pathWithEnv = @"%USERPROFILE%\MyShop\config.json";
            var filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            System.IO.Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\MyShop"));

            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
            string f = "";
            using (var ostream = new StreamReader(fileStream))
            {
                f = ostream.ReadToEnd();
            }
            var json = f.Length != 0 ? JsonNode.Parse(f) : new JsonObject();
            if (f.Length != 0)
            {
                username = json["Username"]!.ToString() ?? "";
                passwordIn64 = json["Password"]!.ToString() ?? "";
                entropyIn64 = json["Entropy"]!.ToString() ?? "";
                RememberMe = (json["RememberMe"]!.ToString() ?? "") == "true";
            }
            else
            {
                json["Username"] = Username;
                json["Password"] = passwordIn64;
                json["Entropy"] = entropyIn64;
                json["RememberMe"] = RememberMe ? "true" : "false";
                json["LastScreen"] = "1";
                using (var ostream = new StreamWriter(new FileStream(filePath, FileMode.Truncate, FileAccess.Write)))
                {
                    ostream.Write(JsonSerializer.Serialize(json));
                }
            }
            _oldRememberMe = RememberMe;


            if (passwordIn64.Length != 0)
            {
                byte[] entropyInBytes = Convert.FromBase64String(entropyIn64);
                byte[] cypherTextInBytes = Convert.FromBase64String(passwordIn64);

                byte[] passwordInBytes = ProtectedData.Unprotect(cypherTextInBytes,
                    entropyInBytes,
                    DataProtectionScope.CurrentUser
                );

                passwordIn64 = Encoding.UTF8.GetString(passwordInBytes);
                Username = username;

                if (RememberMe)
                {
                    Password = new SecureString();
                    foreach (var c in passwordIn64)
                    {
                        Password.AppendChar(c);
                    }
                }
            }
            else
            {
                _noSavedUsername = true;
            }

        }

        private bool CanExecuteLoginCommand(object obj)
        {
            return true;
            
        }


        private void ExecuteLoginCommand(object obj)
        {
            var currentPassword = Encoding.UTF8.GetBytes(new NetworkCredential(string.Empty, Password).Password);
            if (username.Length != 0 && passwordIn64.Length != 0 && (!username.Equals(Username) || (!_oldRememberMe && !passwordIn64.Equals(Encoding.UTF8.GetString(currentPassword)))))
            {
                MessageBox.Show("Incorrect Username or Password", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_oldRememberMe) currentPassword = Encoding.UTF8.GetBytes(passwordIn64);
            if (Username.Length == 0 || currentPassword.Length == 0)
            {
                MessageBox.Show("Username or Password cannot be empty!", "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Debug.WriteLine("password = " + Encoding.UTF8.GetString(currentPassword));
            {
                // Lưu username và pass
                var pathWithEnv = @"%USERPROFILE%\MyShop\config.json";
                var filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
                var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
                string f = "";
                using (var ostream = new StreamReader(fileStream))
                {
                    f = ostream.ReadToEnd();
                }
                var json = JsonNode.Parse(f);

                // Ma hoa mat khau
                var passwordInBytes = currentPassword;
                var entropy = new byte[20];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(entropy);
                }

                var cypherText = ProtectedData.Protect(
                    passwordInBytes,
                    entropy,
                    DataProtectionScope.CurrentUser
                );

                var passwordIn64 = Convert.ToBase64String(cypherText);
                var entropyIn64 = Convert.ToBase64String(entropy);
                json["Username"] = Username;
                json["Password"] = passwordIn64;
                json["Entropy"] = entropyIn64;
                json["RememberMe"] = RememberMe ? "true" : "false";

  
                using (var ostream = new StreamWriter(new FileStream(filePath, FileMode.Truncate, FileAccess.Write)))
                {
                    ostream.Write(JsonSerializer.Serialize(json));
                }
            }
            IsViewVisible = false;
            new MainView().Show();
            App.Current.Windows[0].Close();
        }

        private void ExecuteRememberPassowrdCommand(object obj)
        {
            
        }

        private void ExecuteRecoverPassCommand(string username, string email)
        {
            return;
        }

        public void  UsernameChanged(object sender, EventArgs e)
        {
            var u = (TextBox)sender;
            Username = u.Text;
        }

        public void PasswordChanged(object sender, EventArgs e)
        {
            var p = (PasswordBox)sender;
            Password = new SecureString();
            foreach(var c in p.Password)
            {
                Password.AppendChar(c);
            }
        }
    }
}

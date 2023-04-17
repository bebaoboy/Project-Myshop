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

        private string username;
        private string passwordIn64;
        private string entropyIn64;

        //Constructor
        public LoginViewModel()
        {
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            CheckCommand = new ViewModelCommand(ExecuteRememberPassowrdCommand);

            username = ConfigurationManager.AppSettings["Username"] ?? "";
            passwordIn64 = ConfigurationManager.AppSettings["Password"] ?? "";
            entropyIn64 = ConfigurationManager.AppSettings["Entropy"] ?? "";
            RememberMe = (ConfigurationManager.AppSettings["RememberMe"] ?? "") == "true";
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
                var config = ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

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
                config.AppSettings.Settings["Username"].Value = Username;
                config.AppSettings.Settings["Password"].Value = passwordIn64;
                config.AppSettings.Settings["Entropy"].Value = entropyIn64;
                config.AppSettings.Settings["RememberMe"].Value = RememberMe ? "true" : "false";

                config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");
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

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;

namespace Butik_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Product
    {
        public string Name;
        public int Amount;
        public int Price;
    }

    public class BasketProduct : Product
    {
        public bool IsChecked;
    }

    public class DiscountCoupone
    {
        public string Code;
        public double Percentage;
        public bool IsUsed;
    }

    public partial class MainWindow : Window
    {
        public Grid grid = new Grid();

        public WrapPanel productWrapPanel;
        public StackPanel buttonsPanel;
        public StackPanel basketStackPanel;

        public Button finishButton;
        public Label totalPriceLabel;
        public TextBox discountTextBox;

        public double total;
        public DiscountCoupone Coupone;
        public string savedBasketPath = @"C:\Windows\Temp\SavedBasket.csv";

        public string[] readBasketArray;
        public string[] readProductsArray;
        public string[] readCouponesArray;

        public List<string> savedProductsList = new List<string>();
        public List<BasketProduct> basketItemsList = new List<BasketProduct>();
        public List<Product> buyedProductsList = new List<Product>();
        public List<DiscountCoupone> discountCouponesList = new List<DiscountCoupone>();

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }
        public void Start()
        {
            MainLayout();
            KassaTableTitle();
            GetProducts();
        }

        public void MainLayout()
        {
            Title = "AM Frukt Affär";
            Height = 600;
            Width = 1310;

            ScrollViewer scroll = (ScrollViewer)Content;
            scroll.Content = grid;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel productStackPanel = new StackPanel
            {
                Background = Brushes.LightPink
            };
            grid.Children.Add(productStackPanel);
            Grid.SetColumn(productStackPanel, 0);

            productWrapPanel = new WrapPanel
            {
                Margin = new Thickness(10),
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            productStackPanel.Children.Add(productWrapPanel);

            basketStackPanel = new StackPanel
            {
                Width = 365,
                Background = Brushes.LightGray,
                Margin = new Thickness(10, 0, 0, 0),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            grid.Children.Add(basketStackPanel);
            Grid.SetColumn(basketStackPanel, 1);
        }

        public void GetProducts()
        {
            ReadSavedBasket();

            if (File.Exists("Produkter.csv"))
            {
                readProductsArray = File.ReadAllLines("Produkter.csv");

                foreach (string product in readProductsArray)
                {
                    string[] splittedPruduct = product.Split(',');

                    string image = splittedPruduct[0];
                    string name = splittedPruduct[1];
                    string description = splittedPruduct[2];
                    string price = splittedPruduct[3];

                    CreateProductBlock(image, name, description, price);
                }
            }
            else
            {
                MessageBox.Show("Produktfilen finns inte!");
            }
        }

        public void CreateProductBlock(string imageName, string name, string description, string price)
        {
            StackPanel productStackPanel = new StackPanel()
            {
                Width = 155,
                Height = 250,
                Margin = new Thickness(10),
                Background = Brushes.White,
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            UIElement uie = productStackPanel;
            uie.Effect = new DropShadowEffect
            {
                Opacity = 0.3,
                Direction = 320,
                ShadowDepth = 6,
                Color = new Color { A = 1, R = 0, G = 0, B = 0 }
            };

            string path = ("Bilder/" + imageName);

            ImageSource source = new BitmapImage(new Uri(path, UriKind.Relative));
            Image image = new Image
            {
                Source = source,
                Width = 100,
                Height = 100,
                Stretch = Stretch.Fill,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Label productName = new Label
            {
                Content = name,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10, 20, 0, 0)
            };

            Label productDescription = new Label
            {
                Content = description,
                Padding = new Thickness(10, 0, 0, 0)
            };

            Label productPrice = new Label
            {
                Content = "Pris: " + price + "/kg",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(10, 5, 0, 0),
            };

            StackPanel amountAndBuy = new StackPanel()
            {
                Margin = new Thickness(10),
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button buyButton = new Button()
            {
                Content = "Köp " + name,
                Height = 20,
                Width = 100,
                Margin = new Thickness(5)
            };

            TextBox amount = new TextBox()
            {
                Width = 25,
                Height = 20,
                MaxLength = 3,
                Margin = new Thickness(2),
                TextAlignment = TextAlignment.Center,
            };

            buyButton.Tag = amount;

            amountAndBuy.Children.Add(amount);
            amountAndBuy.Children.Add(buyButton);

            productStackPanel.Children.Add(image);
            productStackPanel.Children.Add(productName);
            productStackPanel.Children.Add(productPrice);
            productStackPanel.Children.Add(productDescription);
            productStackPanel.Children.Add(amountAndBuy);

            productWrapPanel.Children.Add(productStackPanel);

            buyButton.Click += BuyProduct_Click;

            Product buyedProduct = new Product()
            {
                Name = name,
                Amount = 0,
                Price = int.Parse(price),
            };
            buyedProductsList.Add(buyedProduct);
        }
        private void BuyProduct_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            string name = button.Content.ToString().Remove(0, 4);
            Product product = buyedProductsList.Where(a => a.Name == name).Select(a => a).FirstOrDefault();

            TextBox amountTextBox = (TextBox)button.Tag;

            if (int.TryParse(amountTextBox.Text, out int result))
            {
                if (int.Parse(amountTextBox.Text) > 0)
                {
                    int amount = int.Parse(amountTextBox.Text);
                    product.Amount = amount;
                    SendToBasket(name, amount, amount * product.Price);
                    amountTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("Du måste skriva ett nummer som större än noll!");
                }
            }
            else
            {
                MessageBox.Show("Du kan bara skriva heltal och nummer som är större än noll!");
            }
        }

        public void KassaTableTitle()
        {
            string path = ("Bilder/Logo.png");

            ImageSource source = new BitmapImage(new Uri(path, UriKind.Relative));
            Image image = new Image
            {
                Source = source,
                Width = 250,
                Height = 125,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 10, 5, 0)
            };
            basketStackPanel.Children.Add(image);

            Label kassan = new Label()
            {
                Content = "KASSAN: ",
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 0, 5, 0)
            };
            basketStackPanel.Children.Add(kassan);

            TableTitle();
        }

        public void TableTitle()
        {
            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Width = 330,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 5, 0),
            };
            basketStackPanel.Children.Add(panel);

            Label itemNumber = new Label()
            {
                Content = "No.",
                Padding = new Thickness(5),
                FontSize = 15,
                Width = 40,
                FontWeight = FontWeights.Bold
            };
            panel.Children.Add(itemNumber);

            Label itemName = new Label()
            {
                Content = "Produkt",
                Padding = new Thickness(5),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Width = 100
            };
            panel.Children.Add(itemName);

            Label itemAmount = new Label()
            {
                Content = "Mängd",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Width = 90,
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(itemAmount);

            Label itemPrice = new Label()
            {
                Content = "Pris",
                Padding = new Thickness(5),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Width = 70,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(itemPrice);
        }

        public void SendToBasket(string name, int amount, int price)
        {
            BasketProduct basketItem = new BasketProduct()
            {
                Name = name,
                Amount = amount,
                Price = price,
                IsChecked = false
            };

            if (basketItemsList.Exists(a => a.Name == name))
            {
                BasketProduct existProduct = basketItemsList.Where(a => a.Name == name).Select(a => a).FirstOrDefault();
                existProduct.Amount += amount;
                existProduct.Price += price;
            }
            else
            {
                basketItemsList.Add(basketItem);
            }

            ShowBasketItems();
        }

        public void ShowBasketItems()
        {
            int counter = 1;

            basketStackPanel.Children.Clear();
            KassaTableTitle();

            foreach (var item in basketItemsList)
            {
                StackPanel itemPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Width = 330,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                basketStackPanel.Children.Add(itemPanel);

                Label itemNumber = new Label()
                {
                    Content = counter,
                    Width = 40,
                    FontSize = 15,
                    Padding = new Thickness(5)
                };
                itemPanel.Children.Add(itemNumber);

                Label itemName = new Label()
                {
                    Content = item.Name,
                    Width = 100,
                    FontSize = 15,
                    Padding = new Thickness(5)
                };
                itemPanel.Children.Add(itemName);

                Label itemAmount = new Label()
                {
                    Content = item.Amount,
                    Width = 90,
                    FontSize = 15,
                    Padding = new Thickness(5),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };
                itemPanel.Children.Add(itemAmount);

                Label itemPrice = new Label()
                {
                    Content = item.Price + " kr",
                    Width = 70,
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                itemPanel.Children.Add(itemPrice);

                CheckBox checkBox = new CheckBox()
                {
                    VerticalAlignment = VerticalAlignment.Center
                };
                itemPanel.Children.Add(checkBox);

                checkBox.Tag = item;

                checkBox.Checked += CheckBox_IsChecked;
                checkBox.Unchecked += CheckBox_IsChecked;

                counter++;
            }
            if (basketItemsList.Count != 0)
            {
                CreateButtons();
            }
        }

        private void CheckBox_IsChecked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            BasketProduct item = (BasketProduct)box.Tag;

            if (box.IsChecked == true)
            {
                item.IsChecked = true;
            }
            else
            {
                item.IsChecked = false;
            }
        }

        public void CreateButtons()
        {
            StackPanel buttons = new StackPanel()
            {
                Margin = new Thickness(5),
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            basketStackPanel.Children.Add(buttons);

            Button save = new Button()
            {
                Content = "Spara",
                Width = 70,
                Height = 30,
                FontSize = 15,
                Margin = new Thickness(5),
            };
            buttons.Children.Add(save);
            save.Click += Save_Click;

            Button delete = new Button()
            {
                Content = "Ta Bort",
                Width = 70,
                Height = 30,
                FontSize = 15,
                Margin = new Thickness(5),
            };
            buttons.Children.Add(delete);
            delete.Click += Delete_Click;

            Button pay = new Button()
            {
                Content = "Betala",
                Width = 70,
                Height = 30,
                FontSize = 15,
                Margin = new Thickness(5),
            };
            buttons.Children.Add(pay);
            pay.Click += Pay_Click;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in basketItemsList)
            {
                string name = item.Name;
                string totalPrice = item.Price.ToString();
                string totalAmount = item.Amount.ToString();

                savedProductsList.Add(name + "," + totalAmount + "," + totalPrice);
            }
            File.Delete(savedBasketPath);
            File.WriteAllLines(savedBasketPath, savedProductsList);
            MessageBox.Show("Din varukorg har sparats!");

            savedProductsList.Clear();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            ReadDiscountCuopones();
            PaymentDetails();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (basketItemsList.All(a => a.IsChecked == false))
            {
                MessageBox.Show("Du måste välj en produkt!");
            }
            else
            {
                foreach (var item in basketItemsList.ToList())
                {
                    if (item.IsChecked == true)
                    {
                        if(basketItemsList.Count == 1)
                        {
                            basketItemsList.Remove(item);
                            File.Delete(savedBasketPath);

                        }
                        else
                        {
                            basketItemsList.Remove(item);
                        }
                    }
                }
                ShowBasketItems();
            }
        }

        public void PaymentDetails()
        {
            int counter = 1;

            basketStackPanel.Children.Clear();

            Label receiptTitle = new Label()
            {
                Content = "Din Faktura:",
                FontSize = 20,
                Margin = new Thickness(10),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            basketStackPanel.Children.Add(receiptTitle);

            TableTitle();

            foreach (var item in basketItemsList)
            {
                StackPanel panel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Width = 330,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                basketStackPanel.Children.Add(panel);

                Label itemNumber = new Label()
                {
                    Content = counter,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 40
                };
                panel.Children.Add(itemNumber);

                Label itemName = new Label()
                {
                    Content = item.Name,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 100
                };
                panel.Children.Add(itemName);

                Label itemAmount = new Label()
                {
                    Content = item.Amount,
                    Padding = new Thickness(5),
                    FontSize = 15,
                    Width = 90,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };
                panel.Children.Add(itemAmount);

                Label itemPrice = new Label()
                {
                    Content = item.Price + " kr",
                    FontSize = 15,
                    Width = 70,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                panel.Children.Add(itemPrice);

                counter++;
            }

            total = basketItemsList.Select(a => a.Price).Sum();

            totalPriceLabel = new Label()
            {
                Content = "Ditt totala belopp är: " + total + " kr",
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10)
            };

            Label discount = new Label
            {
                Content = "Skriv in din rabattkod:",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 10, 10, 0)
            };
            basketStackPanel.Children.Add(discount);

            StackPanel discountStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            basketStackPanel.Children.Add(discountStack);

            discountTextBox = new TextBox
            {
                Margin = new Thickness(2),
                TextAlignment = TextAlignment.Left,
                Width = 150,
                Height = 25,
                Padding = new Thickness(5)
            };

            Button discountButton = new Button
            {
                Content = "Lös kod",
                Width = 60,
                Height = 25,
                Margin = new Thickness(5)
            };

            discountStack.Children.Add(discountTextBox);
            discountStack.Children.Add(discountButton);
            basketStackPanel.Children.Add(totalPriceLabel);

            discountButton.Click += DiscountButton_Click;

            buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            basketStackPanel.Children.Add(buttonsPanel);

            Button previous = new Button()
            {
                Content = "Tillbaka",
                Width = 75,
                Height = 30,
                Margin = new Thickness(10, 0, 10, 0)
            };
            buttonsPanel.Children.Add(previous);
            previous.Click += Brevious_Click;

            finishButton = new Button()
            {
                Content = "Slutför",
                Width = 75,
                Height = 30,
                Margin = new Thickness(10, 0, 10, 0)
            };
            buttonsPanel.Children.Add(finishButton);
            finishButton.Click += Finish_Click;
        }

        private void Brevious_Click(object sender, RoutedEventArgs e)
        {
            basketStackPanel.Children.Clear();
            KassaTableTitle();
            ShowBasketItems();
        }

        private void DiscountButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateDiscount(discountTextBox.Text, totalPriceLabel);
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Grattis, ditt köp är klart!");
            basketStackPanel.Children.Clear();
            basketItemsList.Clear();
            File.Delete(savedBasketPath);
            KassaTableTitle();
        }

        public void ReadSavedBasket()
        {
            if (File.Exists(savedBasketPath))
            {
                readBasketArray = File.ReadAllLines(savedBasketPath);

                if (readBasketArray.Length > 0)
                {
                    foreach (var item in readBasketArray)
                    {
                        string[] temp = item.Split(',');

                        BasketProduct basketItem = new BasketProduct();

                        basketItem.Name = temp[0];
                        basketItem.Amount = int.Parse(temp[1]);
                        basketItem.Price = int.Parse(temp[2]);
                        basketItem.IsChecked = false;
                        basketItemsList.Add(basketItem);
                    }
                    ShowBasketItems();
                }
            }
        }

        public void ReadDiscountCuopones()
        {
            string[] discount = File.ReadAllLines("Discount.csv");

            foreach (var item in discount)
            {
                readCouponesArray = item.Split(',');
                DiscountCoupone coupone = new DiscountCoupone()
                {
                    Code = readCouponesArray[0],
                    Percentage = double.Parse(readCouponesArray[1]),
                    IsUsed = false
                };
                discountCouponesList.Add(coupone);
            }
        }

        public void CalculateDiscount(string input, Label totalprice)
        {
            bool Valid = false;

            foreach (var code in discountCouponesList)
            {
                if (input == code.Code && code.IsUsed == false)
                {
                    Valid = true;
                    code.IsUsed = true;

                    break;
                }
            }

            if (discountCouponesList.Where(s => s.Code == input).Select(s => s).FirstOrDefault() != null)
            {
                Coupone = discountCouponesList.Where(s => s.Code == input).Select(s => s).FirstOrDefault();

                if (Coupone.IsUsed == true && Valid == false)
                {
                    MessageBox.Show("Den kod är redan använd");
                }
                else if (Valid == true && total > 0)
                {
                    double discountedTotal = Math.Round(total - (total * Coupone.Percentage / 100), 2);
                    totalprice.Content = "Ditt totala belopp är: " + discountedTotal + " kr";
                    total = discountedTotal;

                    basketStackPanel.Children.Remove(totalPriceLabel);
                    basketStackPanel.Children.Add(totalPriceLabel);
                    basketStackPanel.Children.Remove(buttonsPanel);
                    basketStackPanel.Children.Add(buttonsPanel);
                    Valid = false;

                    MessageBox.Show("Din rabatt kod är accepterad!");
                }
            }

            if (discountCouponesList.Count(a => a.Code == input) == 0)
            {
                MessageBox.Show("Ogiltig rabatt kod!");
            }

            discountTextBox.Clear();
        }
    }
}

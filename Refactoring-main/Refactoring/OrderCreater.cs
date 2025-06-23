using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace SimpleOrderApp
{
    public class OrderCreater
    {
        private string customerName; // Поле для хранения имени клиента
        public string Subject { get; private set; } // Для хранения темы письма
        public string Body { get; private set; } // Для хранения тела письма
        public NetworkCredential Credentials { get; private set; }

        public void ProcessOrder(string customerName, List<string> items, string paymentMethod)
        {
            // Сохраняем имя клиента в поле класса
            this.customerName = customerName;

            // Проверка входных данных и пустых значений 
            if (string.IsNullOrEmpty(customerName))
                throw new ArgumentException("Укажите имя клиента");
            if (items == null || items.Count == 0)
                throw new ArgumentException("Добавьте товары в заказ");
            // Расчет суммы
            decimal total = CalculateTotal(items);
            // Применение скидки
            if (items.Count > 2) total *= 0.9m;
            // Обработка платежа
            ProcessPayment(paymentMethod, total);
            // Логирование
            LogOrder(total);
            // Отправка email уведомления
            SendOrderEmail(total);
        }
        private decimal CalculateTotal(List<string> items) // Здесь поменял на лист потому что в него удобнее добовлять новые значения
                                                           // и не надо бесконечно писать else if. Изначально думал написать try catch, но потом понял что с ним тоже как то не удобно именно в этом проекте:)
        {
            var prices = new Dictionary<string, decimal>
            {
                {"Ноутбук", 1200},
                {"Мышь", 25},
                {"Клавиатура", 50},
                {"Камера", 500},
                {"Колонки", 150}
            };
            // Так же рассчёт цены находится в этом же методе. Потому что до этого итоговая цена рассчитывалась в каждом else if, а теперь мы просто считаем ключ из листа
            // и в зависимости от него считаем итоговую цену. Вообще конечно вроде можно было это сделать по "колхозу" не заморачиваясь и выводить сразу и ключ и цену.
            decimal total = 0;
            foreach (var item in items)
            {
                if (prices.TryGetValue(item, out decimal price))
                {
                    total += price;
                }
                else
                {
                    throw new ArgumentException($"Неизвестный товар: {item}");
                }
            }
            return total;
        }
        private void ProcessPayment(string method, decimal amount) // Тут соответственно метод с проверкой оплаты. Его по сути то и не менял просто скобочки поубирал и MessageBox добавил
        {
            if (method == "По карте")
                MessageBox.Show($"Оплата картой: {amount} руб");
            else if (method == "PayPal")
                MessageBox.Show($"PayPal оплата: {amount} руб");
            else
                throw new ArgumentException("Неизвестный способ оплаты");
        }
        private void LogOrder(decimal total) // Логика превратилась в метод
        {
            string logEntry = $"Заказ обработан для {this.customerName} в {DateTime.Now}";
            Directory.CreateDirectory(@".\logs"); // Создаем папку, если её нет
            File.WriteAllText(@".\logs\order.txt", logEntry);
        }
        private void SendOrderEmail(decimal total) // Вынес отправку email в отдельный метод вообще не уверен что это работает\
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                Credentials = new NetworkCredential("lomovala@gmail.com", "your_password");// Замените на реальный пароль
                var message = new MailMessage
                {
                    Subject = "Новый заказ",
                    Body = $"<h2>Новый заказ для {this.customerName} общей стоимостью {total:C}.</h2>",
                };
                message.To.Add("lomovala@gmail.com");
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                // Смотрим ошибку отправки почты, но не прерываем работу
                File.AppendAllText(@".\logs\email_errors.log", $"{DateTime.Now} Ошибка отправки email: {ex.Message}\n");
            }
        }
    }
}
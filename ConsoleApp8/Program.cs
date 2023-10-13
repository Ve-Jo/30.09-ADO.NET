using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Library;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                ListDebtors(conn);
                GetAuthorOfBook(conn, 3);
                ListAvailableBooks(conn);
                GetUserBook(conn, 2);
                ListBooksTakenLast2Weeks(conn);
                ClearDebts(conn);
                CountBooksForStudentLastYear(conn, 3);
            }
        }

        static void ListDebtors(SqlConnection conn)
        {
            Console.WriteLine("#1 Список должников из dbo.Student (dbo.S_Cards), которые всё ещё не вернули книгу:");

            using (SqlCommand cmd = new SqlCommand("SELECT s.first_name, s.last_name FROM dbo.S_Cards sc JOIN dbo.Student s ON s.id = sc.id_student WHERE sc.date_in IS NULL", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["first_name"]} {reader["last_name"]}");
                }
            }
        }

        static void GetAuthorOfBook(SqlConnection conn, int bookId)
        {
            Console.WriteLine("#2 Автор книги #3 из dbo.Book (dbo.Author):");

            using (SqlCommand cmd = new SqlCommand("SELECT a.first_name, a.last_name FROM dbo.Book b JOIN dbo.Author a ON b.id_author = a.id WHERE b.id = @bookId", conn))
            {
                cmd.Parameters.AddWithValue("@bookId", bookId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine($"Автор: {reader["first_name"]} {reader["last_name"]}");
                    }
                }
            }
        }

        static void ListAvailableBooks(SqlConnection conn)
        {
            Console.WriteLine("#3 Список всех доступных книг (dbo.Book):");

            using (SqlCommand cmd = new SqlCommand("SELECT b.name, b.quantity FROM dbo.Book b WHERE b.quantity > 0", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Книга: {reader["name"]} ({reader["quantity"]})");
                }
            }
        }

        static void GetUserBook(SqlConnection conn, int studentId)
        {
            Console.WriteLine("#4 Вывод книги (dbo.Book), которая была у пользователя #2:");

            using (SqlCommand cmd = new SqlCommand("SELECT b.name FROM dbo.S_Cards sc JOIN dbo.Book b ON sc.id_book = b.id WHERE sc.id_student = @studentId", conn))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine($"Книга: {reader["name"]}");
                    }
                    else
                    {
                        Console.WriteLine("Нет книги");
                    }
                }
            }
        }

        static void ListBooksTakenLast2Weeks(SqlConnection conn)
        {
            Console.WriteLine("#5 Список книг (dbo.Book), взятые за последние 2 недели:");

            //Так-как книги брались ещё раньше, лучше поставить DATEADD(YEAR, -6, GETDATE()) что-бы было видно данные
            using (SqlCommand cmd = new SqlCommand("SELECT b.name FROM dbo.S_Cards sc JOIN dbo.Book b ON sc.id_book = b.id WHERE sc.date_in IS NULL AND sc.id_student IN (SELECT id_student FROM dbo.S_Cards WHERE date_out BETWEEN DATEADD(WEEK, -2, GETDATE()) AND GETDATE())", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Книга: {reader["name"]}");
                }
            }
        }

        static void ClearDebts(SqlConnection conn)
        {
            Console.WriteLine("#6 Обнуление задолженности:");

            //Все NULL заменяются на текущее время. При повторном запуске больше должников не будет
            using (SqlCommand cmd = new SqlCommand("UPDATE dbo.S_Cards SET date_in = GETDATE() WHERE date_in IS NULL", conn))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Проверьте данные в dbo.S_Cards");
        }

        static void CountBooksForStudentLastYear(SqlConnection conn, int studentId)
        {
            Console.WriteLine("#7 Кол-во книг определённого человека за последний год:");

            //Так-как книги брались ещё раньше, лучше поставить DATEADD(YEAR, -6, GETDATE()) что-бы было видно данные
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) AS num_books FROM dbo.S_Cards sc WHERE sc.id_student = @studentId AND sc.date_out BETWEEN DATEADD(YEAR, -1, GETDATE()) AND GETDATE()", conn))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine($"Студент взял {reader["num_books"]} книжек.");
                    }
                    else
                    {
                        Console.WriteLine("Студент не брал книг");
                    }
                }
            }
        }
    }
}

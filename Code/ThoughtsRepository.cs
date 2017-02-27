using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace MyWpfForismatic
{
    public class ThoughtsRepository
    {
        // string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public static List<Authors> authors_list = new List<Authors>();
        public static List<Thought> thought_list = new List<Thought>();

        public static bool IsDataLoaded = false;

        public ThoughtsRepository()
        { }

        //
        //--------     Загружаем с сервера мысли и авторов (очищая при этом предыдущие данные)    --------------->>>
        //
        #region public static void LoadDataFromServer()
        /// <summary>
        /// Загружаем с сервера мысли и авторов (очищая при этом предыдущие данные)
        /// </summary>
        public static void LoadDataFromServer()
        {
            authors_list.Clear();
            thought_list.Clear();

            //  Cинхронное чтение  SELECT * FROM Authors,    SELECT * FROM Thoughts 
            //
            int cnt = ReadDataFromServer();
        }
        #endregion public static void LoadDataFromServer()

        //
        //----------------------     Cинхронное чтение  SELECT * FROM Authors,    SELECT * FROM Thoughts    ---------------------->>>
        //
        #region private static int ReadDataFromServer()

        /// <summary>
        /// Cинхронное чтение  SELECT * FROM Authors,    SELECT * FROM Thoughts
        /// </summary>
        /// <param name="sender">Ссылка на объект родительского класса - для работы с его открытими членами</param>
        /// <returns></returns>
        private static int ReadDataFromServer()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            int rows_read = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    Console.WriteLine("Строка подключения: " + connection.ConnectionString);

                    connection.Open();
                    Console.WriteLine("Подключение открыто");

                    //
                    // Read data from table Authors
                    //
                    // string sqlExpression = "SELECT * FROM Authors ORDER BY AuthorName";

                    /*
-- =============================================
-- Author:		Victor
-- Create date: 17-may-2016
-- Description:	Select list of Authors and total count of thoughts for these authors from SqlServer
-- =============================================
alter PROCEDURE SelectAuthorsAndThoughtsCount
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- DELETE Thoughts WHERE Id = @Id

	SELECT a.Id,
	cnt = (SELECT COUNT(*) from Thoughts group by AuthorId having AuthorId = a.Id),
	AuthorName
	FROM Authors a	
	WHERE (SELECT COUNT(*) from Thoughts group by AuthorId having AuthorId = a.Id) IS NOT NULL
	ORDER BY AuthorName
END
                    */

                    // string sqlExpression = "SELECT * FROM Authors ORDER BY AuthorName";
                    string sqlExpression = "SelectAuthorsAndThoughtsCount";


                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure; //++
                    Console.WriteLine("SqlDataReader reader = await command.ExecuteReaderAsync()...");

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        // выводим названия столбцов
                        // Console.WriteLine("{0}\t{1}\t{2}", reader.GetName(0), reader.GetName(1), reader.GetName(2));
                        Console.WriteLine("{0}\t{1}", reader.GetName(0), reader.GetName(1));

                        Console.WriteLine("--- Первые 2 записи из таблицы Authors : ---");

                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            int cnt = reader.GetInt32(1);
                            string name = reader.GetString(2);

                            if (rows_read < 2)
                                Console.WriteLine("{0} \t{1} \t{2}", id, cnt, name);

                            // add data to list
                            //
                            Authors new_author = new Authors(id, cnt, name);
                            authors_list.Add(new_author);

                            //++
                            rows_read++;
                        }
                        Console.WriteLine("--- was read {0} rows ---", rows_read);
                    }
                    reader.Close();

                    //
                    // Read data from table Authors
                    //
                    /*
-- =============================================
-- Author:		Victor
-- Create date: 17-may-2016
-- Description:	Select list of Thoughts from SqlServer
-- =============================================
CREATE PROCEDURE SelectThoughts
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	-- DELETE Thoughts WHERE Id = @Id

	SELECT * FROM Thoughts ORDER BY AuthorId, Id
END
                    */
                    // sqlExpression = "SELECT * FROM Thoughts ORDER BY AuthorId, Id";
                    sqlExpression = "SelectThoughts";
                    command = new SqlCommand(sqlExpression, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    Console.WriteLine("SqlDataReader reader = await command.ExecuteReaderAsync()...");

                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        rows_read = 0;

                        // выводим названия столбцов
                        Console.WriteLine("{0}\t{1}\t{2}\t{3}", reader.GetName(0), reader.GetName(1), reader.GetName(2), reader.GetName(3));

                        Console.WriteLine("--- Первые 2 записи из таблицы Thoughts : ---");

                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string text = reader.GetString(1);
                            int author_id = reader.GetInt32(2);
                            string link = reader.GetString(3);

                            if (rows_read < 2)
                                Console.WriteLine("{0} \t{1} \t{2} \t{3}", id, text, author_id, link);

                            // add data to list
                            //
                            Thought new_thought = new Thought(id, text, author_id, link);
                            thought_list.Add(new_thought);

                            //++
                            rows_read++;
                        }
                        Console.WriteLine("--- was read {0} rows ---", rows_read);
                    }
                    reader.Close();

                }
                //++ all data was loaded
                IsDataLoaded = true;
                Console.WriteLine("--- data from SQL Server was loaded !!! ---");
            }
            catch (Exception x)
            {
                rows_read = -1;
                Console.WriteLine("ERROR #001 : " + x.Message);
            }
            Console.WriteLine("...Подключение закрыто.");

            return rows_read;
        }
        #endregion private static int ReadDataFromServer()

        //
        //----------------------     Извлекаем список мыслей определенного автора    ---------------------->>>
        //
        #region public static List<Thought> GetThoughtsByAuthorId()

        /// <summary>
        /// Извлекаем список мыслей определенного автора
        /// </summary>
        /// <param name="author_id">Id искомого автора</param>
        /// <returns></returns>
        public static List<Thought> GetThoughtsByAuthorId(int author_id = 1)
        {
            List<Thought> ret_list = thought_list.Where(th => th.AuthorId == author_id).ToList();
            return ret_list;
        }
        #endregion public static List<Thought> GetThoughtsByAuthorId()

        //
        //----------------------     Извлекаем Id определенного автора по его имени   ---------------------->>>
        //
        #region public static int GetAuthorIdByAuthorName()

        /// <summary>
        /// Извлекаем Id определенного автора по его имени
        /// </summary>
        /// <param name="author_name">Имя искомого автора</param>
        /// <returns>AuthorId для найденного автора (если не найден, то -1)</returns>
        public static int GetAuthorIdByAuthorName(string author_name)
        {
            int author_id = -1;
            try
            {
                // List<Thought> ret_list = thought_list.Where(th => th.AuthorId == author_id).ToList();
                //
                Authors cur_author = authors_list.First(a => a.AuthorName.Equals(author_name, StringComparison.CurrentCultureIgnoreCase));
                if (cur_author != null)
                    author_id = cur_author.Id;
            }
            catch (Exception x)
            {
                return -1;
            }
            return author_id;
        }
        #endregion public static int GetAuthorIdByAuthorName()

        //
        //----------------------     Cинхронно добавляем новую мысль в базу данных    ---------------------->>>
        //
        #region public static bool AddRecordToSqlServer()

        /// <summary>
        /// Cинхронно добавляем новую мысль в базу данных
        /// </summary>
        /// <param name="forismatic">Добавляемая мысль (в виде перменной типа Forismatic)</param>
        /// <returns>Id of added record to table Thoughts </returns>
        public static int AddRecordToSqlServer(Forismatic forismatic)
        {
            int id = -1;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            /*
 -- =============================================
-- Author:		Victor
-- Create date: 02-may-2016
-- Description:	Insert new thought to SqlServer
-- Return: Id of added record to table Thoughts
-- =============================================
ALTER PROCEDURE [dbo].[InsertForismaticToServer]
    -- Add the parameters for the stored procedure here
    @Text nvarchar(500), @Author nvarchar(250), @Link nvarchar(200), @Id int output
AS
             */
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    Console.WriteLine("Строка подключения: " + connection.ConnectionString);

                    connection.Open();
                    Console.WriteLine("Подключение открыто");

                    // название процедуры
                    string sqlExpression = "InsertForismaticToServer";

                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    Console.WriteLine("SqlDataReader reader = await command.ExecuteReaderAsync()...");

                    // указываем, что команда представляет хранимую процедуру
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // параметр для ввода самой умной мысли
                    SqlParameter param1 = new SqlParameter
                    {
                        ParameterName = "@Text",
                        Value = forismatic.Text.Trim()
                    };
                    // добавляем параметр
                    command.Parameters.Add(param1);

                    // параметр для ввода имени автора
                    SqlParameter param2 = new SqlParameter
                    {
                        ParameterName = "@Author",
                        Value = forismatic.Author.Trim()
                    };
                    command.Parameters.Add(param2);

                    // параметр для ввода ссылки на мысль
                    SqlParameter param3 = new SqlParameter
                    {
                        ParameterName = "@Link",
                        Value = forismatic.Link.Trim()
                    };
                    command.Parameters.Add(param3);

                    // Id of added record to table Thoughts
                    SqlParameter param4 = new SqlParameter
                    {
                        ParameterName = "@Id",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output // параметр выходной
                    };
                    command.Parameters.Add(param4);


                    // var result = command.ExecuteScalar();
                    command.ExecuteNonQuery();
                    Console.WriteLine("Id добавленной в таблицу Thoughts мысли: {0}", param4.Value);

                    //++
                    if (int.TryParse(param4.Value.ToString(), out id))
                    {
                        //++
                        // Plays the sound associated with the Asterisk system event.
                        SystemSounds.Asterisk.Play();

                        //++ all data was loaded
                        IsDataLoaded = true;
                        Console.WriteLine("--- Data was added to table Thoughts, Id of added record = " + id.ToString());
                    }
                    else
                    {
                        SystemSounds.Hand.Play();

                        //++ all data was loaded
                        IsDataLoaded = true;
                        Console.WriteLine("--- Error while adding record to table Thoughts !!! ---");
                    }
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("ERROR #001 : " + x.Message);

                //++
                // Plays the sound associated with the Asterisk system event.
                // SystemSounds.Asterisk.Play();
                SystemSounds.Hand.Play();
            }
            Console.WriteLine("...Подключение закрыто.");
            return id;
        }
        #endregion public static bool AddRecordToSqlServer()

        //
        //-----------     Cинхронно Удаляем мысль из базы данных (и автора, если у него нет больше мыслей)   ----------------->>>
        //
        #region public static bool DeleteRecordFromSqlServer()

        /// <summary>
        /// Cинхронно Удаляем мысль из базы данных (и автора, если у него нет больше мыслей)
        /// </summary>
        /// <param name="id">Идентификатор удаляемой мысли</param>
        /// <returns></returns>
        public static bool DeleteRecordFromSqlServer(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            /*
             * CREATE PROCEDURE [dbo].[DeleteThoughtFromServer]
             * 	-- Add the parameters for the stored procedure here
             * 		@Id int
             * 		AS
             */
            bool ret_val = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    Console.WriteLine("Строка подключения: " + connection.ConnectionString);

                    connection.Open();
                    Console.WriteLine("Подключение открыто");

                    // название процедуры
                    string sqlExpression = "DeleteThoughtFromServer";

                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    Console.WriteLine("SqlDataReader reader = await command.ExecuteReaderAsync()...");

                    // указываем, что команда представляет хранимую процедуру
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // параметр для ввода идентификатора удаляемой умной мысли
                    SqlParameter param1 = new SqlParameter
                    {
                        ParameterName = "@Id",
                        Value = id
                    };
                    // добавляем параметр
                    command.Parameters.Add(param1);


                    // var result = command.ExecuteScalar();
                    // если нам не надо возвращать id
                    //var result = command.ExecuteNonQuery();

                    var result = command.ExecuteScalar();
                    // Console.WriteLine("Id добавленной в таблицу Thoughts мысли: {0}", result);

                    //++
                    // Plays the sound associated with the Asterisk system event.
                    SystemSounds.Asterisk.Play();
                    // SystemSounds.Hand.Play();
                }
                //++ all data was loaded
                IsDataLoaded = true;
                Console.WriteLine("--- data from SQL Server was loaded !!! ---");
                ret_val = true;
            }
            catch (Exception x)
            {
                Console.WriteLine("ERROR #001 : " + x.Message);
                ret_val = false;

                //++
                // Plays the sound associated with the Asterisk system event.
                // SystemSounds.Asterisk.Play();
                SystemSounds.Hand.Play();
            }
            Console.WriteLine("...Подключение закрыто.");
            return ret_val;
        }
        #endregion public static bool DeleteRecordFromSqlServer()

    }
}

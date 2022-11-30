using System;
using System.Data;
using MySql.Data.MySqlClient;

public class dbManager
{
	string conString;

	MySqlConnection con;
	MySqlCommand cmd;
	MySqlDataAdapter adapter;

	DataSet ds;

	public dbManager(string conString)
	{
		this.conString = conString;
	}

	public bool testDB(string query)
	{
		try
		{
            con = new MySqlConnection(conString);
            cmd = new MySqlCommand(query, con);

            con.Open();

			return con.State == ConnectionState.Open ? true : false;
        }
		catch(Exception ex)
		{
			MessageBox.Show(ex.ToString());
			return false;
		}

    }

	public DataSet getStudents(string query)
	{
		return execQuery(query);
	}

	public DataSet getClasses(string query)
	{
		return execQuery(query);
    }

	public DataSet getStudentsByHomeroom(string query) {
		return execQuery(query);
    }

	private DataSet execQuery(string query)
	{
        con = new MySqlConnection(conString);
        cmd = new MySqlCommand(query, con);

        con.Open();

        adapter = new MySqlDataAdapter();
        adapter.SelectCommand = cmd;

        ds = new DataSet();

        adapter.Fill(ds);

        return ds;
    }
}

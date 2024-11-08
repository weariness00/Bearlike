// db.js
import mysql from 'mysql2/promise';
import dotenv from 'dotenv';

dotenv.config();

const pool = mysql.createPool({
  host: process.env.DB_HOST,
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_DATABASE,
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0
});

export async function query(sql, params) {
  try
  {
    const [results, ] = await pool.execute(sql, params);
    return results;
  }
  catch(e)
  {
    return false; 
  }
}

export async function TableVesrionData(tableName)
{
  return await query(`
    SELECT *
    FROM  bearlike.\`table version\` tv 
    WHERE tv.\`Table Name\` = '${tableName}';
    `);
}
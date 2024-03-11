const express = require('express')
const app = express();
const { query } = require('./db');
const PORT = process.env.PORT || 3000;

app.listen(PORT, '0.0.0.0',() => {
  console.log(`Server is running on http://localhost:${PORT}`);
});

const length = 4;

const DonwloadList = '/DownloadList';
const DefaultKeySetting = '/KeySetting/Default';
const StageLootingTable = '/LootingTable/Stage';
const MonsterLootingTable = '/LootingTable/Monster';
const URLList = [DonwloadList, DefaultKeySetting, StageLootingTable, MonsterLootingTable]

const DonwloadListQuery = query("SELECT * FROM bearlike.download");
const KeySettingQuery = query("SELECT * FROM bearlike.keysetting");
const MonsterLootingTableQuery = query("SELECT * FROM bearlike.monster_looting_table");
const StageLootingTableQuery = query("SELECT * FROM bearlike.stage_looting_table");
const QueryList = [DonwloadListQuery, KeySettingQuery, MonsterLootingTableQuery, StageLootingTableQuery]

for (let i = 0; i < length; i++) {
    app.get(URLList[i], async (req,res) => await LoadSQL(req,res, QueryList[i]));
}

async function LoadSQL (req, res, q)
{
    try {
        const results = await q;
        res.json(results);
    } catch (error) {
        console.error('Database query error:', error);
        res.status(500).send('Server error');
    }
}

async function LoadSQLTableList()
{
    try {
        const [rows] = await connection.query('SHOW TABLES', []);
        return rows;
    } catch (error) {
        console.error('Error fetching tables:', error);
    }
}
import express from 'express';
import { query } from './db.js'; // 수정된 부분
import * as Skill from './Skill.js';
import * as Item from './Item.js';

const app = express();
const PORT = process.env.PORT || 3000;

app.listen(PORT, '0.0.0.0',() => {
  console.log(`Server is running on http://localhost:${PORT}`);
});

const length = 4;
const MatachingRunning = false;

const DonwloadList = '/DownloadList';
const DefaultKeySetting = '/KeySetting/Default';
const StageLootingTable = '/LootingTable/Stage';
const MonsterLootingTable = '/LootingTable/Monster';
const URLList = [DonwloadList, DefaultKeySetting, MonsterLootingTable, StageLootingTable]

async function DonwloadListQuery(){ return await query("SELECT * FROM bearlike.download");}
async function KeySettingQuery(){return await query("SELECT * FROM bearlike.keysetting");}
async function MonsterLootingTableQuery(){return await query("SELECT * FROM bearlike.monster_looting_table");}
async function StageLootingTableQuery() {return await query("SELECT * FROM bearlike.stage_looting_table");}
const QueryList = [DonwloadListQuery, KeySettingQuery, MonsterLootingTableQuery, StageLootingTableQuery]

for (let i = 0; i < length; i++) {
    app.get(URLList[i], async (req,res) => await LoadSQL(req,res, QueryList[i]));
}

Skill.MakeSkillData(app);
Item.MakeItemData(app);

async function LoadSQL (req, res, q)
{
    try {
        const results = await q();
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
const express = require('express')
const app = express();
const { query } = require('./db');
const PORT = process.env.PORT || 3000;
import * as matching from './MatchingRoom.js';
import * as Skill from './Skill.js';

app.listen(PORT, '0.0.0.0',() => {
  console.log(`Server is running on http://localhost:${PORT}`);
});

const length = 4;
const MatachingRunning = false;

const DonwloadList = '/DownloadList';
const DefaultKeySetting = '/KeySetting/Default';
const StageLootingTable = '/LootingTable/Stage';
const MonsterLootingTable = '/LootingTable/Monster';
const MathcingRoom = "/Room Information"
const URLList = [DonwloadList, DefaultKeySetting, StageLootingTable, MonsterLootingTable, MathcingRoom]

async function DonwloadListQuery(){ return query("SELECT * FROM bearlike.download");}
async function KeySettingQuery(){return query("SELECT * FROM bearlike.keysetting");}
async function MonsterLootingTableQuery(){return query("SELECT * FROM bearlike.monster_looting_table");}
async function StageLootingTableQuery() {return query("SELECT * FROM bearlike.stage_looting_table");}
// const MatchingRoomQuery = () => {
//     if(MatachingRunning)
//     {
//         return MatachingRunning
//     }
//     else
//     {
//         return query("SELECT * FROM bearlike.Matching Room");
//     }
// }
const QueryList = [DonwloadListQuery, KeySettingQuery, MonsterLootingTableQuery, StageLootingTableQuery]

for (let i = 0; i < length; i++) {
    app.get(URLList[i], async (req,res) => await LoadSQL(req,res, QueryList[i]));
}

Skill.MakeSkillData(app);

// setInterval(matching.GCMathcingRoom, 100000);

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
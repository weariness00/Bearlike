import express from 'express';
import { query } from './db.js'; // 수정된 부분
import * as Skill from './Skill.js';
import * as Item from './Item.js';
import * as Monster from './Monster.js';
import * as Stage from './Stage.js';

const app = express();
const PORT = process.env.PORT || 3000;

app.listen(PORT, '0.0.0.0',() => {
  console.log(`Server is running on http://localhost:${PORT}`);
});

const length = 2;
const MatachingRunning = false;

const DonwloadList = '/DownloadList';
const DefaultKeySetting = '/KeySetting/Default';
const URLList = [DonwloadList, DefaultKeySetting]

async function DonwloadListQuery(){ return await query("SELECT * FROM bearlike.download");}
async function KeySettingQuery(){return await query("SELECT * FROM bearlike.keysetting");}
const QueryList = [DonwloadListQuery, KeySettingQuery]

app.get('/DataTime', async (req, res) => {
    const now = new Date();
    const currentTimeInSeconds = Math.floor(now.getTime() / 1000); // 밀리초를 초로 변환
    res.send(currentTimeInSeconds);
})

for (let i = 0; i < length; i++) {
    app.get(URLList[i], async (req,res) => await LoadSQL(req,res, QueryList[i]));
}

Skill.MakeSkillData(app);
Item.MakeItemData(app);

Monster.MakeMonsterStatusData(app);
Monster.MakeLootingTable(app);

Stage.MakeLootingTable(app);

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
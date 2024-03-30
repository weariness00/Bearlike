import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function LootingTableQuery()
{
    return await query(
        `SELECT 
        s.ID  as ID,
        s.\`Type\` as Type,
        JSON_ARRAYAGG(
          JSON_OBJECT(
            \'Item ID\', lt.\`Item ID\`,
            \'Probability\', lt.Probability,
            \'Amount\', lt.Amount,
            \'Is Networked\', lt.\`Is Networked\` 
          )
        ) as LootingTable
      FROM stage_looting_table lt
      JOIN stage s ON lt.\`Stage ID\` = s.ID  
      JOIN item i ON lt.\`Item ID\`  = i.ID
      GROUP BY lt.\`Stage ID\`;`
    );
}

async function MakeInfoData(app)
{
    app.get('/Stage/Version', async (req, res) => {
        var version = await TableVesrionData("Stage");
        res.json(version);
    });

    var json = await query('SELECT * FROM bearlike.stage');
    app.get('/Stage', async (req, res) =>{
        res.json(json);
    });

    json.forEach(data => {
        var id = data.ID;
        app.get(`/Stage/${id}`, (req, res) =>{
            res.json(data);
        });
    });
}

async function MakeLootingTable(app)
{
    app.get('/Stage/LootingTable/Version', async (req, res) => {
        var version = await TableVesrionData("Stage Looting Table");
        res.json(version);
    });

    var json = await LootingTableQuery();
    app.get('/Stage/LootingTable', async (req, res) => {
        try {
            res.json(json);
        } catch (error) {
            res.status(500).send('Item Query error');
        }
    })  
    json.forEach(data => {
        var id = data.ID;
        app.get(`/Stage/LootingTable/${id}`, async (req,res) => {
            try {
                res.json(data);
            } catch (error) {
                res.status(500).send('Item Query error' + id);
            }
        })
    });
}

export async function MakeData(app)
{
    MakeInfoData(app);
    MakeLootingTable(app);
}
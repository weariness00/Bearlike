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

    app.get('/Stage', async (req, res) =>{
        var json = await query('SELECT * FROM bearlike.stage');
        res.json(json);
    });

    app.get(`/Stage/ID/:id`, async (req, res) =>{
        var id = req.params.id;
        var json = await query('SELECT * FROM bearlike.stage');
        var data = json.find(s => s.ID == id);

        res.json(data);
    });
}

async function MakeLootingTable(app)
{
    app.get('/Stage/LootingTable/Version', async (req, res) => {
        var version = await TableVesrionData("Stage Looting Table");
        res.json(version);
    });

    app.get('/Stage/LootingTable', async (req, res) => {
        var json = await LootingTableQuery();
        res.json(json);
    })  
            
    app.get(`/Stage/LootingTable/id/:id`, async (req,res) => {
        var id = req.params.id;
        var json = await LootingTableQuery();
        var data = json.find(table => table.ID == id);
        res.json(data);
    })
}

export async function MakeData(app)
{
    MakeInfoData(app);
    MakeLootingTable(app);
}
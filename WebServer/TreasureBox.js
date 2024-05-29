import { query, TableVesrionData } from './db.js'; // 수정된 부분

async function InfoQuery()
{
    return await query(
        `
SELECT 
    tb.ID as 'ID',
    tb.\`Explain\` as 'Explain',
    JSON_ARRAYAGG(
    JSON_OBJECT(
      'Open Condition Type', tc.\`Type\`,
      'Item ID', tc.\`Item ID\`,
      'Money Amount', tc.\`Money Amount\`,
      'Open Condition Explain', tc.Explain
    )
  ) as 'Condition',
    JSON_ARRAYAGG(
    JSON_OBJECT(
      'Item ID', tl.ItemID,
      'Probability', tl.Probability,
      'Amount', tl.Amount,
      'Is Networked', tl.\`Is Networked\` 
    )
  ) as 'LootingTable'
FROM 
    treasurebox_looting_table tl
JOIN 
    treasurebox_condition tc ON tl.\`Treasure Box ID\` = tc.\`Treasure Box ID\`
JOIN 
    treasurebox tb ON tl.\`Treasure Box ID\` = tb.ID 
JOIN 
    item i ON tl.ItemID = i.ID
GROUP BY tb.ID;
`
    );
}

async function MakeInfoData(app)
{
    app.get('/TreasureBox/Version', async (req, res) => {
        var version = await TableVesrionData("TreasureBox");
        res.json(version);
    })

    app.get('/TreasureBox', async (req, res) => { 
        var json = await InfoQuery();
        res.json(json); 
    })
    app.get(`/TreasureBox/id/:id`, async (req, res) => {
        var id = req.params.id;
        var json = await InfoQuery();
        var data = json.find(s => s.ID == id);
        
        res.json(data);
    });
}

export async function MakeData(app)
{
    await MakeInfoData(app);
}
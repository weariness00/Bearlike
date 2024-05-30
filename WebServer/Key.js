import { query } from './db.js';

async function KeySettingQuery(){return await query("SELECT * FROM bearlike.keysetting");}

export function MakeDefaultKeyData(app)
{
    app.get('/KeySetting/Default', async (req,res) => {
        var json = await KeySettingQuery();
        res.json(json);
    });
}

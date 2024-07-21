import express from 'express';
import * as Key from './Key.js';
import * as Skill from './Skill.js';
import * as Item from './Item.js';
import * as Monster from './Monster.js';
import * as Stage from './Stage.js';
import * as Weapon from './Weapon.js';
import * as TreasureBox from './TreasureBox.js';
import * as Difficult from './Difficult.js';
import * as MagicCotton from './MagicCotton.js';

const app = express();
const PORT = process.env.PORT || 3000;

app.listen(PORT, '0.0.0.0',() => {
  console.log(`Server is running on http://localhost:${PORT}`);
});

Key.MakeDefaultKeyData(app);

// 난이도
Difficult.MakeData(app);

Skill.MakeData(app);

// 아이템
Item.MakeData(app);

// 몬스터
Monster.MakeData(app);

Stage.MakeData(app);

Weapon.MakeData(app);

TreasureBox.MakeData(app);

MagicCotton.MakeData(app);

- Input System Setting:

![](../pictures/InputSystem.png)

- animator control

	- 动画来源于 mixamo 的 idle walk run
	- 基于 一维 blend tree 实现的停走跑切换动画
	- 基于 character controller 实现的位移

![|300](../pictures/Animation1.png)
![|300](../pictures/Animation2.png)

啊呀所以动画速度和移动速度会有出入，我把参数写在面板里了（辛苦策划！）

![|400](../pictures/3rd.png)

- walk speed / run speed: 字面意思，位移
- Gravity：y方向上的人造重力，这个参数很怪
- Rotation...: 转向的灵敏程度（大概）

- Acceleration / Deceleration: 
	- 按 w 一段时间之后才能从停到走，从走到跑
	- Acceleration / Deceleration 影响切换速度
Character Outline
=================
The character outline effect plays a role in highlighting and highlighting the character in NPR. In particular, this project, which aims at rendering such as illustration, has focused on drawing the outline as naturally as possible. Accordingly, the target point for this outline shader has been set as follows
>  Depth Normal Outline
>
> Draw Character Only

To achieve this, I have set up an implementation plan as follows.
![Alt text](/ExplainImgs/ShaderImplementionPlanMap.png)

### Character Camera Setting

First, after dividing the character layers, set up a character camera that will render only the characters. Because I will render only the outline of the character. 

First, set the rendering layer. In Inspector > Layer > Add Layer, add Layer named "Character" and Assign to the character.
![Alt text](/ExplainImgs/ShaderImplementionPlanMap.png)
![Alt text](/ExplainImgs/AssignLayerToCharacter.png)

After that, create another camera as children of the main camera. And set it up as follows.
![Alt text](/ExplainImgs/AddCharacterCam.png)
![Alt text](/ExplainImgs/CharacterCamSettings.png)

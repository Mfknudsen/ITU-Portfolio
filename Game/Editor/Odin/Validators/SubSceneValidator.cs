#if UNITY_EDITOR
using Editor.Odin.Validators;
using Sirenix.OdinInspector.Editor.Validation;
using Unity.Scenes;

[assembly: RegisterValidator(typeof(SubSceneValidator))]

namespace Editor.Odin.Validators
{
    public class SubSceneValidator : ValueValidator<SubScene>
    {
        protected override void Validate(ValidationResult result)
        {
            SubScene subScene = this.Value;

            if (subScene.SceneAsset == null)
                result.AddError("Missing required SceneAsset");
        }
    }
}
#endif
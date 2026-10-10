using Foundation;
using UIKit;

namespace StayHub.Mobile;

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
    public override void WillConnect(
        UIScene scene,
        UISceneSession session,
        UISceneConnectionOptions connectionOptions)
    {
        base.WillConnect(scene, session, connectionOptions);

        // The scene's window is available only after MAUI connects it.
        if (Window is not { } window)
            return;

        var tapGesture = new UITapGestureRecognizer(() => window.EndEditing(true))
        {
            CancelsTouchesInView = false
        };

        window.AddGestureRecognizer(tapGesture);
    }
}

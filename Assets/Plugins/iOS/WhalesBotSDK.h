//
//  AppIOSSDK.h
//  Unity-iPhone
//
//  Created by xy on 2018/11/1.
//

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#import <AVFoundation/AVFoundation.h>
@interface AppIOSSDK : NSObject <CLLocationManagerDelegate>
@property (strong) NSString *gameobjectname;
@property (strong)CLLocationManager *_lm;
@property (strong)NSString *_latitude;
@property (strong)NSString *_longitude;
@property (strong)AVAudioSession *_audioSession;
+(AppIOSSDK *)I;
-(void) InitSDK:(NSString *)gameobjectname MethodName:(NSString *)method;
-(void)getGPS;
-(NSString *)getSystemLanguage;
//会话取消回调
- (void) onCancel;
@end

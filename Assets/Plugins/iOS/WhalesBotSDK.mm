//
//  AppIOSSDK.m
//  Unity-iPhone
//
//  Created by xy on 2018/11/1.
//

#import "AppIOSSDK.h"
#import <iflyMSC/IFlyMSC.h>
#define IS_IOS8 ([[[UIDevice currentDevice] systemVersion] floatValue] >= 8)
@implementation AppIOSSDK 
static AppIOSSDK * _instance = nil;

+(AppIOSSDK *)I{
    if (_instance == nil) {
        _instance = [[AppIOSSDK alloc] init];
    }
    return _instance;
}


-(void) InitSDK:(NSString *)gameobjectname{
    [AppIOSSDK I].gameobjectname = gameobjectname;
}

+ (NSString *)stringFromABNFJson:(NSString*)params
{
    if (params == NULL) {
        return nil;
    }
    NSMutableString *tempStr = [[NSMutableString alloc] init];
    NSDictionary *resultDic  = [NSJSONSerialization JSONObjectWithData:
                                [params dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    
    NSArray *wordArray = [resultDic objectForKey:@"ws"];
    for (int i = 0; i < [wordArray count]; i++) {
        NSDictionary *wsDic = [wordArray objectAtIndex: i];
        NSArray *cwArray = [wsDic objectForKey:@"cw"];
        
        for (int j = 0; j < [cwArray count]; j++) {
            NSDictionary *wDic = [cwArray objectAtIndex:j];
            NSString *str = [wDic objectForKey:@"w"];
            NSString *score = [wDic objectForKey:@"sc"];
            [tempStr appendString: str];
            [tempStr appendFormat:@" Confidence:%@",score];
            [tempStr appendString: @"\n"];
        }
    }
    return tempStr;
}

-(NSString *)getSystemLanguage{
    NSArray *appLanguages = [[NSUserDefaults standardUserDefaults] objectForKey:@"AppleLanguages"];
    NSString *languageName = [appLanguages objectAtIndex:0];
    if ([languageName containsString:@"TW"]) {
        languageName = @"zh_tw";
    }else if(([languageName containsString:@"HK"])){
        languageName = @"zh_hk";
    }else if([languageName containsString:@"MO"]){
        languageName = @"zh_mo";
    }else if([languageName containsString:@"zh"]){
        languageName = @"zh_cn";
    }else{
        languageName = @"en";
    }
    return languageName;
}
-(void)getGPS{
    if ([CLLocationManager locationServicesEnabled]) {
        [AppIOSSDK I]._lm = [[CLLocationManager alloc]init];
        [AppIOSSDK I]._lm.delegate = [AppIOSSDK I];
        [AppIOSSDK I]._lm.desiredAccuracy = kCLLocationAccuracyBest;
        [AppIOSSDK I]._lm.distanceFilter = 100.f;
        if (IS_IOS8) {
            [[AppIOSSDK I]._lm requestWhenInUseAuthorization];
        }
        [[AppIOSSDK I]._lm startUpdatingLocation];
    }else{
        UnitySendMessage([[AppIOSSDK I].gameobjectname UTF8String],"GPSCallBack","");
    }
}

-(int)getVoice{
    if ([AppIOSSDK I]._audioSession == NULL) {
        [AppIOSSDK I]._audioSession = [AVAudioSession sharedInstance];
    }
    CGFloat currentVol = [AppIOSSDK I]._audioSession.outputVolume;
    return (int)currentVol;
}

- (void)exitApplication {
    UIWindow *window = [UIApplication sharedApplication].delegate.window;
    [UIView animateWithDuration:1.0f animations:^{
        window.alpha = 0;
        window.frame = CGRectMake(0, window.bounds.size.width, 0, 0);
    } completion:^(BOOL finished) {
        exit(0);
    }];
}

- (void)locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray *)locations{
    CLLocation *currLocation = [locations firstObject];
    [AppIOSSDK I]._latitude = [NSString stringWithFormat:@"%3.5f",currLocation.coordinate.latitude];
    [AppIOSSDK I]._longitude = [NSString stringWithFormat:@"%3.5f",currLocation.coordinate.longitude];
    NSString *gps = [NSString stringWithFormat:@"%@,%@",[AppIOSSDK I]._latitude,[AppIOSSDK I]._longitude];
    UnitySendMessage([[AppIOSSDK I].gameobjectname UTF8String],"GPSCallBack",[gps UTF8String]);
    [[AppIOSSDK I]._lm stopUpdatingLocation];
}

extern "C"{
    void InitSDK(char * gameobjectname){
        [[AppIOSSDK I] InitSDK:[NSString stringWithFormat:@"%s", gameobjectname]];
    }
    
    
    void StartListening(char *lang){
        [[AppIOSSDK I] StartListening:[NSString stringWithFormat:@"%s", lang]];
    }
    
    void StopListening(){
        [[AppIOSSDK I] StopListening];
    }
    
    char * GetIOSLanguage(){
        return strdup([[[AppIOSSDK I] getSystemLanguage] UTF8String]);
    }
    
    void GetGPS(){
        [[AppIOSSDK I] getGPS];
    }
    
    int GetIOSVoice(){
        return [[AppIOSSDK I] getVoice];
    }
    
    void ExitApplication(){
        [[AppIOSSDK I] exitApplication];
    }
}
@end

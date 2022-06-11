import React, {useEffect, useRef, useState} from 'react';
import {Dropdown, Icon, Menu} from 'semantic-ui-react';
import {Link} from 'react-router-dom';
import {useDispatch, useSelector} from 'react-redux';
import {useTranslation} from 'react-i18next';
import {
    dataLoadingMenuSelector,
    dictionariesHeaderSelector,
    dictionariesMenuSelector,
    gridsMenuSelector,
    otherMenuSelector,
    reportsMenuSelector,
    rolesAndUsersMenu,
    roleSelector,
    userNameSelector,
} from '../../ducks/profile';
import useReactRouter from 'use-react-router';
import {isAuthSelector, logoutRequest} from '../../ducks/login';
import './style.scss';
import {DICTIONARY_LIST_LINK, GRID_LIST_LINK, PROFILE_LINK, REPORT_LINK} from '../../router/links';
import {
    dataLoadingRequest,
    getInstructionRequest,
    progressLoadingDataSelector,
    templateUploadRequest,
} from '../../ducks/loadingData';
import {downloader} from "../../utils/postman";
import downloadFile from "../../utils/downloadFile";

const Header = () => {
    const dispatch = useDispatch();
    const grids = useSelector(state => gridsMenuSelector(state));
    const dictionariesList = useSelector(state => dictionariesMenuSelector(state));
    const dictionariesMenu = useSelector(state => dictionariesHeaderSelector(state));
    const otherMenu = useSelector(state => otherMenuSelector(state));
    const usersAndRoles = useSelector(state => rolesAndUsersMenu(state));
    const dataLoadingMenu = useSelector(state => dataLoadingMenuSelector(state));
    const reportMenu = useSelector(state => reportsMenuSelector(state));
    const userName = useSelector(state => userNameSelector(state));
    const userRole = useSelector(state => roleSelector(state));
    const isAuth = useSelector(state => isAuthSelector(state));
    const loading = useSelector(state => progressLoadingDataSelector(state));
    const {t} = useTranslation();
    const {location, history} = useReactRouter();
    const fileUploader = useRef(null);

    let [activeItem, setActiveItem] = useState(location.pathname);
    let [currentTypeApi, setCurrentTypeApi] = useState(null);

    useEffect(() => {
        setActiveItem(location.pathname);
    }, [location.pathname]);

    const logOut = () => {
        dispatch(logoutRequest());
    };

    const handleOpenProfile = () => {
        history.push(PROFILE_LINK);
    };

    const handleClickLoadData = (type, typeApi, fileName) => {
        console.log('`${window.location.origin}/static/${fileName}`', `${window.location.origin}/api/static/${fileName}`);
        if (type === 'unloading') {
            dispatch(templateUploadRequest({
                typeApi
            }));
        } else if (type === 'loading') {
            setCurrentTypeApi(typeApi);
            fileUploader && fileUploader.current.click();
        } else if (type === 'instruction') {
            dispatch(getInstructionRequest({
                fileName
            }))
        }
    };

    const handleClickReport = (type) => {
        history.push(REPORT_LINK.replace(':type', type));
    };

    const onFilePicked = e => {
        const file = e.target.files[0];

        const data = new FormData();
        data.append('FileName', file.name);
        data.append('FileContent', new Blob([file], {type: file.type}));
        data.append('FileContentType', file.type);

        dispatch(
            dataLoadingRequest({
                form: data,
                typeApi: currentTypeApi
            }),
        );
        setCurrentTypeApi(null);
        fileUploader.current.value = null;
    };

    return (
        <>
            {isAuth ? (
                <header>
                    <Menu pointing secondary fixed="top" style={{paddingLeft: '12px'}}>
                        {/*<Menu.Item>LOGO</Menu.Item>*/}
                        {grids &&
                        grids.map(item => (
                            <Menu.Item
                                className="large"
                                key={item}
                                as={Link}
                                to={GRID_LIST_LINK.replace(':name', item)}
                                name={item}
                                active={activeItem.includes(item)}
                            >
                                {t(item)}
                            </Menu.Item>
                        ))}
                        {dictionariesMenu && dictionariesMenu.length
                            ? dictionariesMenu.map(item => (
                                <Menu.Item
                                    className="large"
                                    key={item}
                                    as={Link}
                                    to={DICTIONARY_LIST_LINK.replace(':name', item)}
                                    name={item}
                                    active={activeItem.includes(item)}
                                >
                                    {t(item)}
                                </Menu.Item>
                            ))
                            : null}
                        {otherMenu && otherMenu.length
                            ? otherMenu.map(item => (
                                <Menu.Item
                                    className="large"
                                    key={item.name}
                                    as={Link}
                                    to={item.link}
                                    name={item.name}
                                    active={activeItem.includes(item.name)}
                                >
                                    {t(item.name)}
                                </Menu.Item>
                            ))
                            : null}
                        {(dictionariesList && dictionariesList.length) || usersAndRoles.length ? (
                            <Menu.Menu>
                                <Dropdown
                                    text={t('dictionaries')}
                                    item
                                    className={`${[
                                        ...dictionariesList,
                                        ...usersAndRoles.map(item => item.name),
                                    ].some(x => activeItem.includes(x)) && 'superActive'}`}
                                >
                                    <Dropdown.Menu>
                                        {dictionariesList.map(item => {
                                            return (
                                                <Dropdown.Item
                                                    key={item}
                                                    as={Link}
                                                    to={DICTIONARY_LIST_LINK.replace(':name', item)}
                                                    active={activeItem.includes(item)}
                                                    name={item}
                                                >
                                                    {t(item)}
                                                </Dropdown.Item>
                                            );
                                        })}
                                        {usersAndRoles.map(item => (
                                            <Dropdown.Item
                                                key={item.name}
                                                as={Link}
                                                to={item.link}
                                                active={activeItem.includes(item.name)}
                                                name={item.name}
                                            >
                                                {t(item.name)}
                                            </Dropdown.Item>
                                        ))}
                                    </Dropdown.Menu>
                                </Dropdown>
                            </Menu.Menu>
                        ) : null}
                        {reportMenu && reportMenu.length ? (
                            <Menu.Menu>
                                <Dropdown text={t('Reports')} item>
                                    <Dropdown.Menu>
                                        {reportMenu.map(item => (
                                            <Dropdown.Item
                                                key={item.name}
                                                name={item.name}
                                                text={t(item.name)}
                                                onClick={() => handleClickReport(item.type)}
                                            />
                                        ))}
                                    </Dropdown.Menu>
                                </Dropdown>
                            </Menu.Menu>
                        ) : null}
                        {dataLoadingMenu && dataLoadingMenu.length ? (
                            <Menu.Menu>
                                <Dropdown text={t('data_loading')} item loading={loading}>
                                    <Dropdown.Menu>
                                        {dataLoadingMenu.map(item => (
                                            <Dropdown
                                                item
                                                className="link item"
                                                key={item.name}
                                                name={item.name}
                                                text={t(item.name)}
                                            >
                                                <Dropdown.Menu>
                                                    {item.items.map(childItem => (
                                                        <Dropdown.Item
                                                            key={childItem.name}
                                                            name={childItem.name}
                                                            text={t(childItem.name)}
                                                            onClick={() =>
                                                                handleClickLoadData(
                                                                    childItem.type,
                                                                    item.typeApi,
                                                                    childItem.fileName
                                                                )
                                                            }
                                                        />))}
                                                </Dropdown.Menu>
                                            </Dropdown>
                                        ))}
                                    </Dropdown.Menu>
                                </Dropdown>
                            </Menu.Menu>
                        ) : null}
                        <div className="header-support">
                            <Icon name="question circle"/>
                            <div className="header-support_contacts">
                                <a href="mailto:support@pooling.me">support@pooling.me</a>
                                <div>{t('support_work_time')}</div>
                            </div>
                            {userName && userRole ? (
                                <Menu.Menu>
                                    <Dropdown text={`${userName} (${userRole})`} item>
                                        <Dropdown.Menu>
                                            <Dropdown.Item onClick={handleOpenProfile}>
                                                {t('profile_settings')}
                                            </Dropdown.Item>
                                            <Dropdown.Item onClick={logOut}>
                                                {t('exit')}
                                            </Dropdown.Item>
                                        </Dropdown.Menu>
                                    </Dropdown>
                                </Menu.Menu>
                            ) : null}
                        </div>
                    </Menu>
                </header>
            ) : null}
            <input
                type="file"
                ref={fileUploader}
                style={{display: 'none'}}
                onChange={onFilePicked}
            />
        </>
    );
};
Header.propTypes = {};

export default Header;

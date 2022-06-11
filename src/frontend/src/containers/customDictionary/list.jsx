import React, {Component} from 'react';
import {connect} from 'react-redux';
import {withRouter} from 'react-router-dom';
import {withTranslation} from 'react-i18next';

import { Confirm } from 'semantic-ui-react';
import TableInfo from '../../components/TableInfo';
import {
    canCreateByFormSelector,
    canExportToExcelSelector,
    canImportFromExcelSelector,
    clearDictionaryInfo,
    columnsSelector,
    exportProgressSelector,
    exportToExcelRequest,
    getListRequest,
    importFromExcelRequest,
    importProgressSelector,
    listSelector,
    progressSelector,
    totalCountSelector,
} from '../../ducks/dictionaryView';
import {DICTIONARY_CARD_LINK, DICTIONARY_NEW_LINK} from '../../router/links';


class List extends Component {
    state = {
        confirmation: {
            open: false
        }
    };

    componentWillUnmount() {
        this.props.clear();
    }

    closeConfirmation = () => {
        this.setState({
            confirmation: {
                open: false,
            }
        });
    };

    showConfirmation = (content, onConfirm, onCancel) => {
        this.setState({
            confirmation: {
                open: true,
                content,
                onConfirm,
                onCancel,
            }
        });
    };

    handleImportFromExcel = (form, callbackSuccess) => {
        const { importFromExcel, match } = this.props;
        const { params = {} } = match;
        const { name = '' } = params;

        const callbackConfirmation = (message) => {
            this.showConfirmation(
                message,
                () => {
                    this.closeConfirmation();

                    importFromExcel({
                        form,
                        name,
                        callbackSuccess,
                        isConfirmed: true
                    });
                },
                this.closeConfirmation
            )
        };

        importFromExcel({
            form,
            name,
            callbackSuccess,
            isConfirmed: false,
            callbackConfirmation
        });
    };

    handleExportToExcel = filter => {
        const { exportFromExcel, match } = this.props;
        const { params = {} } = match;
        const { name = '' } = params;
        exportFromExcel({
            name,
            filter,
        });
    };


    render() {
        const {
            match = {},
            columns,
            loadList,
            progress,
            totalCount,
            list,
            isCreateBtn,
            isImportBtn,
            isExportBtn,
            importLoader,
            exportLoader,
            clear,
            t,
        } = this.props;
        const { params = {} } = match;
        const { name = '' } = params;

        return (
            <>
                <TableInfo
                    key={name}
                    headerRow={columns}
                    name={name}
                    className={
                        columns.length >= 10
                            ? 'container'
                            : 'wider ui container container-margin-top-bottom'
                    }
                    loadList={loadList}
                    loading={progress}
                    totalCount={totalCount}
                    title={name}
                    list={list}
                    clear={clear}
                    storageSortItem={`${name}Sort`}
                    storageFilterItem={`${name}Filters`}
                    isImportBtn={isImportBtn}
                    isExportBtn={isExportBtn}
                    importFromExcel={this.handleImportFromExcel}
                    exportToExcel={this.handleExportToExcel}
                    importLoader={importLoader}
                    exportLoader={exportLoader}
                    newLink={isCreateBtn ? DICTIONARY_NEW_LINK : null}
                    cardLink={DICTIONARY_CARD_LINK}
                />
                <Confirm
                    dimmer="blurring"
                    open={this.state.confirmation.open}
                    onCancel={this.state.confirmation.onCancel || this.closeConfirmation}
                    cancelButton={t('cancelConfirm')}
                    confirmButton={t('Yes')}
                    onConfirm={this.state.confirmation.onConfirm}
                    content={this.state.confirmation.content}
                />
            </>
        );
    }
}

const mapStateToProps = (state, ownProps) => {
    const { match = {} } = ownProps;
    const { params = {} } = match;
    const { name = '' } = params;

    return {
        columns: columnsSelector(state, name),
        progress: progressSelector(state),
        totalCount: totalCountSelector(state),
        list: listSelector(state),
        isCreateBtn: canCreateByFormSelector(state, name),
        isImportBtn: canImportFromExcelSelector(state, name),
        isExportBtn: canExportToExcelSelector(state, name),
        importLoader: importProgressSelector(state),
        exportLoader: exportProgressSelector(state),
    };
};

const mapDispatchToProps = dispatch => {
    return {
        loadList: params => {
            dispatch(getListRequest(params));
        },
        importFromExcel: params => {
            dispatch(importFromExcelRequest(params));
        },
        exportFromExcel: params => {
            dispatch(exportToExcelRequest(params));
        },
        clear: () => {
            dispatch(clearDictionaryInfo());
        },
    };
};

export default withTranslation()(
    withRouter(
        connect(
            mapStateToProps,
            mapDispatchToProps,
        )(List),
    ),
);

import React, { useRef, useState, useCallback } from 'react';
import CellValue from '../../ColumnsValue';
import { Button, Form, Icon, Loader, Modal, Table } from 'semantic-ui-react';
import { toast } from 'react-toastify';
import FormField from '../../BaseComponents';
import { LINK_TYPE } from '../../../constants/columnTypes';
import { ORDERS_GRID } from '../../../constants/grids';
import _ from 'lodash';

const ModalComponent = ({ element, props, children }) => {
    if (!element) {
        return <>{children}</>;
    }
    return React.cloneElement(element, props, children);
};

const BodyCell = ({
    value,
    valueText,
    valueTooltip,
    column,
    loadList,
    indexRow,
    indexColumn,
    modalCard,
    gridName,
    t,
    checkForEditing,
    invokeMassUpdate,
    status,
    rowId,
    rowNumber,
    cardLink,
    goToCard,
}) => {
    const contextRef = useRef(null);

    let [open, setOpen] = useState(false);
    let [valueForm, setValue] = useState(null);
    let [progress, setProgress] = useState(false);

    const copyToClipboard = () => {
        const text = contextRef.current && contextRef.current.textContent;
        navigator.clipboard &&
            navigator.clipboard.writeText(text).then(
                () => {
                    toast.info(t('copied_to_clipboard_success'));
                },
                error => {
                    toast.error(t('copied_to_clipboard_error', { error }));
                },
            );
    };

    const handleClick = (rowId, fieldName, state) => {
        setProgress(true);
        checkForEditing({
            rowId,
            fieldName,
            state,
            forEntity: gridName,
            t,
            callbackSuccess: () => {
                handleOpen();
            },
            callbackFunc: () => {
                setProgress(false);
            },
        });
    };

    const handleOpen = () => {
        setOpen(true);
        setValue(valueText ? { value, name: valueText } : value);
    };

    const handleClose = () => {
        setOpen(false);
        setValue(null);
    };

    const handleSave = () => {
        setProgress(true);
        handleClose();
        invokeMassUpdate({
            ids: [rowId],
            name: gridName,
            field: column.name,
            value: valueForm,
            callbackSuccess: () => {
                loadList(false, true);
                setProgress(false);
            }
        });
    };

    const handleChange = useCallback((e, { value }) => {
        setValue(value);
    }, []);

    const handleCellClick = e => {
        column.type !== LINK_TYPE ? handleClick(rowId, column.name, status) : e.stopPropagation();
    };

    const getModalCard = useCallback(() => {
        return (
            <ModalComponent
                element={modalCard()}
                props={{
                    id: rowId,
                    status,
                    loadList,
                    title: `edit_${gridName}`,
                }}
                key={`modal_${rowId}`}
            />
        );
    }, []);

    return (
        <>
            <Table.Cell className="value-cell">
                <div className="cell-grid">
                    <div
                        className={`cell-grid-value ${value ? '' : 'cell-grid-value_empty'}`}
                        ref={contextRef}
                        onClick={handleCellClick}
                    >
                        <CellValue
                            {...column}
                            indexRow={indexRow}
                            indexColumn={indexColumn}
                            value={value}
                            valueText={valueText}
                            valueTooltip={valueTooltip}
                            width={column.width}
                            gridName={gridName}
                            rowId={rowId}
                            t={t}
                            goToCard={goToCard}
                        />
                    </div>
                    <div>
                        {progress ? (
                            <Loader active={true} size="mini" />
                        ) : (
                            <>
                                {value !== null ? (
                                    <div className="cell-grid-copy-btn">
                                        <Icon
                                            name="clone outline"
                                            size="small"
                                            onClick={copyToClipboard}
                                        />
                                    </div>
                                ) : null}
                            </>
                        )}
                    </div>
                </div>
            </Table.Cell>
            <Modal open={open} size="tiny" closeOnDimmerClick={false}>
                <Modal.Header>
                    {t(`edit_${gridName}`, {
                        number: rowNumber,
                        status: t(status),
                    })}
                </Modal.Header>
                <Modal.Content>
                    <Modal.Description>
                        <Form onSubmit={handleSave}>
                            <FormField {...column} autoFocus value={valueForm} onChange={handleChange} />
                        </Form>
                    </Modal.Description>
                </Modal.Content>
                <Modal.Actions>
                    <Button onClick={handleClose}>{t('cancelConfirm')}</Button>
                    <Button
                        color="primary"
                        disabled={_.isEqual(valueForm, value)}
                        onClick={handleSave}
                    >
                        {t('SaveButton')}
                    </Button>
                </Modal.Actions>
            </Modal>
        </>
    );
};

export default React.memo(
    BodyCell /*, (prevProps, nextProps) => {
    return prevProps.column === nextProps.column &&
        prevProps.loadList === nextProps.loadList &&
        prevProps.indexRow === nextProps.indexRow &&
        prevProps.indexColumn === nextProps.indexColumn &&
        prevProps.modalCard === nextProps.modalCard &&
        prevProps.gridName === nextProps.gridName &&
        prevProps.t === nextProps.t &&
        prevProps.checkForEditing === nextProps.checkForEditing &&
        prevProps.invokeMassUpdate === nextProps.invokeMassUpdate &&
        _.isEqual(prevProps.row, nextProps.row)
}*/,
);
